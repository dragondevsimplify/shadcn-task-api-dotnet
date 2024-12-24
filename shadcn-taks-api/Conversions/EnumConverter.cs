using shadcn_taks_api.Persistence.Entities;
using TaskStatus = shadcn_taks_api.Persistence.Entities.TaskStatus;

namespace shadcn_taks_api.Conversions;

public static class EnumConverter
{
    public static TEnum ConvertToEnum<TEnum>(string value) where TEnum : struct
    {
        if (Enum.TryParse(value, out TEnum result) && Enum.IsDefined(typeof(TEnum), result))
        {
            return result;
        }

        if (typeof(TEnum) == typeof(TaskStatus))
        {
            return (TEnum)(object)TaskStatus.Unknown;
        }

        if (typeof(TEnum) == typeof(TaskPriority))
        {
            return (TEnum)(object)TaskPriority.Unknown;
        }

        throw new ArgumentException("Invalid enum value", nameof(value));
    }
}