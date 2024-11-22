using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reddit;
using Reddit.Dtos;
using Reddit.Models;

namespace Reddit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommunitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommunitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Communities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Community>>> GetCommunities(string? searchKey, string? sortKey, bool? isAscending, int pageNumber = 1, int pageSize = 5)
        {
            var communities = _context.Communities.AsQueryable();

            bool ascending = isAscending ?? true;


            if (searchKey is not null)
            {
                communities = communities.Where(c => c.Name.Contains(searchKey) || c.Description.Contains(searchKey));
            }

            if (sortKey is not null)
            {
                switch (sortKey.ToLower())
                {
                    case "createdat":
                        communities = ascending
                            ? communities.OrderBy(c => c.CreatedAt)
                            : communities.OrderByDescending(c => c.CreatedAt);
                        break;
                    case "postscount":
                        communities = ascending
                            ? communities.OrderBy(c => c.Posts.Count())
                            : communities.OrderByDescending(c => c.Posts.Count());
                        break;
                    case "subscriberscount":
                        communities = ascending
                            ? communities.OrderBy(c => c.Subscribers.Count())
                            : communities.OrderByDescending(c => c.Subscribers.Count());
                        break;
                    default:
                        communities = ascending
                            ? communities.OrderBy(c => c.Id)
                            : communities.OrderByDescending(c => c.Id);
                        break;
                }
            }


            if (pageSize > 50)
            {
                pageSize = 50;
            }
            communities = communities.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return  await communities.ToListAsync();
        }

        // GET: api/Communities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Community>> GetCommunity(int id)
        {
            var community = await _context.Communities.FindAsync(id);

            if (community == null)
            {
                return NotFound();
            }

            return community;
        }

        // PUT: api/Communities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCommunity(int id, Community community)
        {
            if (id != community.Id)
            {
                return BadRequest();
            }

            _context.Entry(community).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommunityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Communities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Community>> PostCommunity(CommunityDto communityDto)
        {
            var community = communityDto.CreateCommunity();
            _context.Communities.Add(community);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCommunity", new { id = community.Id }, community);
        }

        // DELETE: api/Communities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCommunity(int id)
        {
            var community = await _context.Communities.FindAsync(id);
            if (community == null)
            {
                return NotFound();
            }

            _context.Communities.Remove(community);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommunityExists(int id)
        {
            return _context.Communities.Any(e => e.Id == id);
        }
    }
}
