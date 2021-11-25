using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeckoimagesApi.Models;

namespace GeckoimagesApi.Controllers
{
    [Route("api/")]
    [ApiController]
    public class GeckoimagesController : ControllerBase
    {
        private readonly GeckoContext _context;

        public GeckoimagesController(GeckoContext context)
        {
            _context = context;
        }

        // GET: api
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Geckoimage>>> GetGeckoimages()
        {
            return await _context.Geckoimages.ToListAsync();
        }

        // GET: api/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Geckoimage>> GetGeckoimage(string id)
        {
            var geckoimage = await _context.Geckoimages.FindAsync(id);

            if (geckoimage == null)
            {
                return NotFound();
            }

            return geckoimage;
        }

        // GET: api/5
        [HttpGet("highest")]
        public async Task<ActionResult<Geckoimage>> GetHighestGeckoimage()
        {
            var geckoimage = _context.Geckoimages.ToList().Where(a => !a.name.Contains("b")).OrderByDescending(a => a.name).First();

            return geckoimage;
        }
        
        /*
        // PUT: api/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGeckoimage(int id, Geckoimage geckoimage)
        {
            if (id != geckoimage.number)
            {
                return BadRequest();
            }

            _context.Entry(geckoimage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GeckoimageExists(id))
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

        // POST: api
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Geckoimage>> PostGeckoimage(Geckoimage geckoimage)
        {
            _context.Geckoimages.Add(geckoimage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGeckoimage", new { id = geckoimage.number }, geckoimage);
        }

        // DELETE: api/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGeckoimage(int id)
        {
            var geckoimage = await _context.Geckoimages.FindAsync(id);
            if (geckoimage == null)
            {
                return NotFound();
            }

            _context.Geckoimages.Remove(geckoimage);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        */

        private bool GeckoimageExists(string id)
        {
            return _context.Geckoimages.Any(e => e.number == id);
        }
    }
}
