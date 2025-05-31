using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Exceptions;
using Project.Core.Services;
using Project.Dto.Http;
using Project.Dto.Http.Converters;
using Project.Dto.Http.Post;
using Swashbuckle.AspNetCore.Annotations;

namespace Project.HttpServer.Controllers;

[ApiController]
[Route("api/post")]
public class PostController :ControllerBase
{
    private readonly IPostService _postService;
    private readonly ILogger<PostController> _logger;

    public PostController(ILogger<PostController> logger,
        IPostService postService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _postService = postService ?? throw new ArgumentNullException(nameof(postService));
    }

    [HttpGet("{postId:guid}")]
    [SwaggerOperation("getPostById")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PostDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetPost([FromRoute] [Required] Guid postId)
    {
        try
        {
            var post = await _postService.GetPostByIdAsync(postId);

            return Ok(PostConverter.Convert(post));
        }
        catch (PostNotFoundException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status404NotFound, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }

    [HttpPost]
    [SwaggerOperation("createPost")]
    [SwaggerResponse(StatusCodes.Status201Created, type: typeof(PostDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> CreatePost([FromBody] [Required] CreatePostDto newPost)
    {
        try
        {
            var createdPost = await _postService.AddPostAsync(newPost.Title,
                newPost.Salary,
                newPost.CompanyId);

            return StatusCode(StatusCodes.Status201Created, PostConverter.Convert(createdPost));
        }
        catch (PostAlreadyExistsException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }

    [HttpPut]
    [SwaggerOperation("updatePost")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PostDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> UpdatePost([FromBody] [Required] UpdatePostDto updatePost)
    {
        try
        {
            var updatedPost = await _postService.UpdatePostAsync(updatePost.Id,
                updatePost.CompanyId,
                updatePost.Title,
                updatePost.Salary);

            return Ok(PostConverter.Convert(updatedPost));
        }
        catch (PostNotFoundException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status404NotFound, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (PostAlreadyExistsException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }

    [HttpDelete("{postId:guid}")]
    [SwaggerOperation("deletePost")]
    [SwaggerResponse(StatusCodes.Status204NoContent, type: typeof(bool))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> DeletePost([FromRoute] [Required] Guid postId)
    {
        try
        {
            await _postService.DeletePostAsync(postId);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (PostNotFoundException e)
        {
            _logger.LogWarning(e, e.Message);
            return StatusCode(StatusCodes.Status404NotFound, new ErrorDto(e.GetType().Name, e.Message));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }

    [HttpGet("/postsPerCompany/{companyId:guid}")]
    [SwaggerOperation("getPostsByCompanyId")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PostDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorDto))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorDto))]
    public async Task<IActionResult> GetPostsByCompanyId([FromRoute] [Required] Guid companyId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var posts = await _postService.GetPostsByCompanyIdAsync(companyId, pageNumber, pageSize);

            return Ok(posts.Posts.Select(PostConverter.Convert));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorDto(e.GetType().Name, e.Message));
        }
    }
}