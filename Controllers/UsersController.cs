using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DatingApp.API.Data;
using AutoMapper;
using DatingApp.API.Dtos;

namespace DatingApp.API.Controllers
{
    [Authorize]//Need log in
    [Route("api/[controller]")] //api/users
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            this._repo = repo;
            this._mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await this._repo.GetUses();
            var usersToReturn = this._mapper.Map<IEnumerable<UserForListDto>>(users);
            return Ok(users);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await this._repo.GetUser(id);
            var userToReturn = this._mapper.Map<UserForDetailDto>(user);
            return Ok(userToReturn);
        }
    }
}