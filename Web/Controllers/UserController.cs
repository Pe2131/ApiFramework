using AutoMapper;
using Common.Exceptions;
using Data.Repositories;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Dtos.Users;
using Services;
using WebFramework.Api;

namespace Web.Controllers
{
    public class UserController : CrudController<UserDto, UserDto, User, string>
    {
        private readonly IUserRepository userRepository;
        private readonly ILogger<UserController> logger;
        private readonly IJwtService jwtService;
        private readonly IMapper mapper;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<User> signInManager;

        public UserController(IUserRepository userRepository, ILogger<UserController> logger, IJwtService jwtService, IMapper mapper,
            UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager) : base(userRepository, mapper)
        {
            this.userRepository = userRepository;
            this.logger = logger;
            this.jwtService = jwtService;
            this.mapper = mapper;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public override async Task<ActionResult<List<UserDto>>> Get(CancellationToken cancellationToken)
        {
            var users = await userRepository.TableNoTracking.ToListAsync(cancellationToken);
            return Ok(users);
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public override async Task<ApiResult<UserDto>> Get(string id, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            await userManager.UpdateSecurityStampAsync(user);

            return mapper.Map<UserDto>(user);
        }
        /// <summary>
        /// This method generate JWT Token
        /// </summary>
        /// <param name="tokenRequest">The information of token request</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<ActionResult> Token([FromForm] TokenRequest tokenRequest, CancellationToken cancellationToken)
        {
            if (!tokenRequest.grant_type.Equals("password", StringComparison.OrdinalIgnoreCase))
                throw new Exception("OAuth flow is not password.");

            //var user = await userRepository.GetByUserAndPass(username, password, cancellationToken);
            var user = await userManager.FindByNameAsync(tokenRequest.username);
            if (user == null)
                throw new BadRequestException("user name or password was wrong");

            var isPasswordValid = await userManager.CheckPasswordAsync(user, tokenRequest.password);
            if (!isPasswordValid)
                throw new BadRequestException("user name or password was wrong");
            var jwt = await jwtService.GenerateAsync(user);
            return new JsonResult(jwt);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public override async Task<ApiResult<UserDto>> Create(UserDto dto, CancellationToken cancellationToken)
        {

            var user = mapper.Map<User>(dto);
            var result = await userManager.CreateAsync(user, dto.Password);
            if (result.Succeeded)
            {
                dto.Id = user.Id;
                return dto;
            }
            else
            {
                return BadRequest(result.Errors.FirstOrDefault().Description);

            }
        }

        [HttpPut]
        [Authorize]
        public override async Task<ApiResult<UserDto>> Update(string id, UserDto dto, CancellationToken cancellationToken)
        {
            if (!(User.IsInRole("Admin")))
            {
                dto.Id = GetUserId;
            }
            var updateUser = await userManager.FindByIdAsync(dto.Id);
            dto.ToEntity(mapper, updateUser);
            await userManager.UpdateAsync(updateUser);
            if (dto.Password != null & dto.Password != "0")
            {
                await userManager.RemovePasswordAsync(updateUser);
                await userManager.AddPasswordAsync(updateUser, dto.Password);
            }
            return Ok(mapper.Map<UserDto>(updateUser));
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public override async Task<ApiResult> Delete(string id, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(cancellationToken, id);
            var roles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, roles.ToArray());
            return await base.Delete(id, cancellationToken);
        }
        [HttpPost("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(string Id, List<string> Roles)
        {
            var user = await userManager.FindByIdAsync(Id);
            var roles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, roles.ToArray());
            await userManager.AddToRolesAsync(await userManager.FindByIdAsync(Id), Roles);
            return Ok();
        }
        [HttpPost("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserRoles(string Id, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(cancellationToken, Id);
            var roles = await userManager.GetRolesAsync(user);
            return Ok(roles);
        }
        [HttpPost("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(string Id, string Role, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(cancellationToken, Id);
            await userManager.RemoveFromRoleAsync(user, Role);
            return Ok();
        }

        [HttpGet("[action]")]
        [Authorize]
        public IActionResult GetId(string Id, string Role)
        {
            return Ok(base.GetUserId);
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetRoleAsync(CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(cancellationToken, GetUserId);
            var roles = await userManager.GetRolesAsync(user);
            return Ok(string.Join(",", roles));
        }
        [HttpGet("[action]")]
        [Authorize]
        public async Task<UserDto> GetDetailsAsync()
        {
            var user = await userManager.FindByIdAsync(GetUserId);
            return mapper.Map<UserDto>(user);
        }
        [HttpGet("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSystemRoles()
        {
            return Ok(await roleManager.Roles.ToListAsync());
        }
        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
            return Ok();
        }
    }
}
