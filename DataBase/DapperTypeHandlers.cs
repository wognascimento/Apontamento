using System;
using System.Data;
using Dapper;

namespace Apontamento.DataBase
{
    public static class DapperTypeHandlers
    {
        private static bool _configured;

        public static void Configure()
        {
            if (_configured)
                return;

            SqlMapper.AddTypeHandler(new DateTimeHandler());
            SqlMapper.AddTypeHandler(new NullableDateTimeHandler());
            _configured = true;
        }

        private sealed class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
        {
            public override DateTime Parse(object value)
            {
                return value switch
                {
                    DateOnly date => date.ToDateTime(TimeOnly.MinValue),
                    DateTime dateTime => dateTime,
                    _ => Convert.ToDateTime(value)
                };
            }

            public override void SetValue(IDbDataParameter parameter, DateTime value)
            {
                parameter.Value = value;
            }
        }

        private sealed class NullableDateTimeHandler : SqlMapper.TypeHandler<DateTime?>
        {
            public override DateTime? Parse(object value)
            {
                if (value is null or DBNull)
                    return null;

                return value switch
                {
                    DateOnly date => date.ToDateTime(TimeOnly.MinValue),
                    DateTime dateTime => dateTime,
                    _ => Convert.ToDateTime(value)
                };
            }

            public override void SetValue(IDbDataParameter parameter, DateTime? value)
            {
                parameter.Value = value.HasValue ? value.Value : DBNull.Value;
            }
        }
    }
}
