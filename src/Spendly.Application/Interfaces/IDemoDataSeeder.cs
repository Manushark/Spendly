namespace Spendly.Application.Interfaces
{
    public interface IDemoDataSeeder
    {
        Task<int> EnsureDemoUserAndDataAsync();
        Task ResetDemoDataAsync(int userId);
    }
}
