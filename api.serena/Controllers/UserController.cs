using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using api.premier.ActionFilter;
using api.premier.Models;
using biz.serena.Entities;
using biz.serena.Paged;
using biz.serena.Repository;
using biz.serena.Servicies;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace api.premier.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;
        private readonly IUserRepository _userRepository;

        public UserController(
            IMapper mapper,
            ILoggerManager logger,
            IUserRepository userRepository)
        {
            _mapper = mapper;
            _logger = logger;
            _userRepository = userRepository;
        }

        [HttpGet]
        public ActionResult<ApiResponse<List<UserDto>>> GetAll()
        {
            var response = new ApiResponse<List<UserDto>>();

            try
            {
                response.Result = _mapper.Map<List<UserDto>>(_userRepository.GetAll());
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("{pageNumber}/{pageSize}")]
        public ActionResult<ApiResponse<PagedList<UserDto>>> GetPaged(int pageNumber, int pageSize)
        {
            var response = new ApiResponse<PagedList<UserDto>>();

            try
            {
                response.Result = _mapper.Map<PagedList<UserDto>>
                    (_userRepository.GetAllPaged(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = ex.ToString();
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public ActionResult<ApiResponse<UserDto>> GetById(int id)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                response.Result = _mapper.Map<UserDto>(_userRepository.Find(c => c.Id == id));
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpGet("VeryfyEmail", Name = "VeryfyEmail")]
        public ActionResult VeryfyEmail(string email)
        {
            var response = new ApiResponse<dynamic>();

            try
            {
                if (_userRepository.VerifyEmail(email) == "No Exist")
                {
                    response.Success = true;
                    response.Message = "Email not exist";
                    response.Result = email;
                }
                else {
                    response.Success = false;
                    response.Message = "The email already exists, maybe there is a Service Record for this user";
                    response.Result = email;
                }

            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpPost]
        [ServiceFilterAttribute(typeof(ValidationFilterAttribute))]
        public ActionResult<ApiResponse<UserDto>> Create(UserCreateDto item)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                if (_userRepository.Exists(c => c.Email == item.Email))
                {
                    response.Success = false;
                    response.Message = $"Email: { item.Email } Already Exists";
                    return BadRequest(response);
                }

                User user = _userRepository.Add(_mapper.Map<User>(item));
                response.Result = _mapper.Map<UserDto>(user);

                //response.Result = _userRepository.Add(_mapper.Map<User>(item));
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = ex.ToString();
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return StatusCode(201, response);
        }

        [HttpPut("Recovery_password", Name = "Recovery_password")]
        public async Task<ActionResult<ApiResponse<UserDto>>> Recovery_password(string email)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                var _user = _mapper.Map<User>(_userRepository.Find(c => c.Email == email));

                if (_user != null)
                {

                    var guid = Guid.NewGuid().ToString().Substring(0, 7);
                    var password = _userRepository.HashPassword("$" + guid);

                    _user.Password = password;
                    _user.Reset = true;
                    _user.UpdateBy = _user.Id;
                    _user.UpdatedDate = DateTime.Now;
                    
                    _userRepository.Update(_mapper.Map<User>(_user), _user.Id);

                    StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/Email.html"));
                    string body = string.Empty;
                    body = reader.ReadToEnd();
                    body = body.Replace("{username}", _user.Name + " " + _user.LastName + " " + _user.MotherLastName);
                    body = body.Replace("{pass}", "$" + guid);

                    _userRepository.SendMail(_user.Email, body, "Recovery password");

                    response.Result = _mapper.Map<UserDto>(_user);
                    response.Success = true;
                    response.Message = "success";
                }
                else
                {
                    response.Success = false;
                    response.Message = "Hubo un error";

                }

            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = ex.ToString();
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpPut("Change_password", Name = "Change_password")]
        public ActionResult<ApiResponse<UserDto>> Change_password(string email, string password)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                var _user = _mapper.Map<User>(_userRepository.Find(c => c.Email == email));

                if (_user != null)
                {
                    var guid = Guid.NewGuid().ToString().Substring(0, 7);
                    var passwordNew = _userRepository.HashPassword(password);

                    _user.Password = passwordNew;
                    _user.UpdateBy = _user.Id;
                    _user.UpdatedDate = DateTime.Now;
                    _user.Reset = false;
                    _userRepository.Update(_mapper.Map<User>(_user), _user.Id);

                    response.Result = _mapper.Map<UserDto>(_user);
                    response.Result.token = _userRepository.BuildToken(_user);
                    response.Success = true;
                    response.Message = "success";
                }
                else
                {
                    response.Success = false;
                    response.Message = "Usuario y/o contraseña incorrecta";

                }

            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpPost("Login", Name = "Login")]
        public async Task<ActionResult<ApiResponse<UserDto>>> Login(string email, string password)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                var _user =  _mapper.Map<User>(_userRepository.Find(c => c.Email == email));
                //var _assignee = _mapper.Map<List<User>>(_userRepository.GetAllIncluding(c => c.AssigneeInformations));
                if (_user != null)
                {
                    if (_userRepository.VerifyPassword(_user.Password, password))
                    {
                        var userData = _mapper.Map<UserDto>(_user);
                        response.Result = userData;
                        response.Result.token = _userRepository.BuildToken(_user);
                        response.Success = true;
                        response.Message = "success";
                    }
                    else
                    {
                        response.Success = false;
                        response.Message = "Usuario y/o contraseña incorrecta";

                    }
                }
                else
                {
                    response.Success = false;
                    response.Message = "Usuario y/o contraseña incorrecta";

                }

            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpPut("{id}")]
        [ServiceFilterAttribute(typeof(ValidationFilterAttribute))]
        public ActionResult<ApiResponse<UserDto>> Update(int id, UserUpdateDto item)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                var user = _userRepository.Find(c => c.Id == id);

                if (user != null)
                {
                    response.Message = $"User id { id } Not Found";
                    return NotFound(response);
                }

                _mapper.Map<User>(item);
                _userRepository.Save();
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public ActionResult<ApiResponse<UserDto>> Delete(int id)
        {
            var response = new ApiResponse<UserDto>();

            try
            {
                var user = _userRepository.Find(c => c.Id == id);

                if (user == null)
                {
                    response.Message = $"User id { id } Not Found";
                    return NotFound(response);
                }

                _userRepository.Delete(user);
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpPut("Token/{id}", Name = "Token/{id}")]
        [ServiceFilterAttribute(typeof(ValidationFilterAttribute))]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateToken([FromQuery] string token, int id)
        {
            var response = new ApiResponse<UserDto>();
            try
            {
                var user = _userRepository.Find(c => c.Id == id);

                if (user == null)
                {
                    response.Message = $"User id { id } Not Found";
                    return NotFound(response);
                }

                user.FcmToken = token;
                await _userRepository.UpdateAsyn(user, id);
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Success = false;
                response.Message = "Internal server error";
                _logger.LogError($"Something went wrong: { ex.ToString() }");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

    }
}
