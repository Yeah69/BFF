namespace BFF.DataVirtualizingObservableCollection
{
    public interface IBasicDataAccess<out T>
    {
        T[] PageFetch(int offSet, int pageSize);
        int CountFetch();
        T CreatePlaceHolder();
    }
}