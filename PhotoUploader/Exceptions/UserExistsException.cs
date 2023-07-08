namespace PhotoUploader.Exceptions
{
    public class UserExistsException : Exception
    {
        public UserExistsException() { }

        public UserExistsException(string name)
            : base(String.Format("Пользователь с логином {0} уже существует", name))
        {

        }
    }
}
