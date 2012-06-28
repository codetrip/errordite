namespace Errordite.Core.Notifications.Parsing
{
    public interface IStructConverter<T> where T :struct 
    {
        string Convert(T o);

        string Convert(T? o);
    }

    public interface IClassConverter
    {
        string Convert(object o);
    }
}