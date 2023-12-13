using GetBaked.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GetBaked.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        //db dependency
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index() 
        { 
            //fetch all categories from db
            var categories = _context.Categories.ToList();

            //return data as json
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public IActionResult Details(int id) 
        { 
            //chechk for missing id
            if (id == null)
            {
                return BadRequest();
            }

            var category = _context.Categories.FirstOrDefault(c=>c.CategoryId == id);
            // we have an id nut it's not in the db
            if (category == null)
            {
                return NotFound();
            }
            //id found return matching category
            return Ok(category);
        }

    }
}
