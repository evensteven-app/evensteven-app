namespace EvenSteven.Shared.Utils
{
    public class TaskResult<T, E>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public E? Error { get; set; }

        public static TaskResult<T, E> Success(T data)
        {
            return new TaskResult<T, E> { IsSuccess = true, Data = data };
        }

        public static TaskResult<T, E> Failure(E error)
        {
            return new TaskResult<T, E> { IsSuccess = false, Error = error };
        }
    }
}
