namespace ERP.Services
{
    public interface IServices
    {
        string iGregorianToPersian(DateTime? date);
        string iGregorianToPersianDateTime(DateTime? date);

        DateTime ConvertPersianDate(string persianDateString, bool useCurrentTime = true, TimeSpan? specificTime = null);
    }
}
