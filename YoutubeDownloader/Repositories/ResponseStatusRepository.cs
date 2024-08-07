using YoutubeDownloaderCS.Models;
using YoutubeDownloaderCS.Services;

namespace YoutubeDownloaderCS.Repositories
{
    public class ResponseStatusRepository : IResponseStatus
    {
        public ResponseStatus BadRequest(object message)
        {
            return new ResponseStatus(400, message);
        }

        public ResponseStatus Created(object message)
        {
            return new ResponseStatus(201, message);
        }

        public ResponseStatus Custom(int statusCode, object message)
        {
            return new ResponseStatus(statusCode, message);
        }

        public ResponseStatus Forbidden(object message)
        {
            return new ResponseStatus(403, message);
        }

        public ResponseStatus InternalServerError(object message, object Inner)
        {
            var msg = $"Error = {message} \nInner = {Inner}";
            return new ResponseStatus(500, msg);
        }

        public ResponseStatus NoContent()
        {
            return new ResponseStatus(204);
        }

        public ResponseStatus NotFound(object message)
        {
            return new ResponseStatus(404, message);
        }

        public ResponseStatus Ok(object message)
        {
            return new ResponseStatus(200, message);
        }

        public ResponseStatus UnAuthorized(object message)
        {
            return new ResponseStatus(401, message);
        }
    }
}
