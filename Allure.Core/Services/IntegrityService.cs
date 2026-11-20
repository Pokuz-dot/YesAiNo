using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Allure.Core.Services;

/// <summary>Каноническая строка реквизитов + криптографический хеш SHA256.</summary>
public sealed class IntegrityService : IIntegrityService
{
    public string BuildCanonicalString(string orderNum, DateTime orderDate, string counterparty,
        decimal totalPrice, int itemCount)
    {
        ArgumentNullException.ThrowIfNull(orderNum);
        ArgumentNullException.ThrowIfNull(counterparty);
        return string.Join("|",
            orderNum.Trim(),
            orderDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            counterparty.Trim(),
            totalPrice.ToString("F2", CultureInfo.InvariantCulture),
            itemCount.ToString(CultureInfo.InvariantCulture));
    }

    public string ComputeHash(string orderNum, DateTime orderDate, string counterparty,
        decimal totalPrice, int itemCount)
    {
        string canonical = BuildCanonicalString(orderNum, orderDate, counterparty, totalPrice, itemCount);
        byte[] bytes = Encoding.UTF8.GetBytes(canonical);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
