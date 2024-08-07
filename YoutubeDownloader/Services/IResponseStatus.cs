using YoutubeDownloaderCS.Models;

namespace YoutubeDownloaderCS.Services
{
    public interface IResponseStatus
    {
        ResponseStatus Ok(object message);
        ResponseStatus InternalServerError(object message, object Inner);
        ResponseStatus Forbidden(object message);
        ResponseStatus UnAuthorized(object message);
        ResponseStatus NotFound(object message);
        ResponseStatus BadRequest(object message);
        ResponseStatus Created(object message);
        ResponseStatus Custom(int statusCode, object message);
        ResponseStatus NoContent();
    }
}
