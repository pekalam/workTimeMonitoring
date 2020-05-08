using Dapper;
using System;
using System.Data;

namespace Infrastructure.Db
{
    internal class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.Value = value;
        }

        public override DateTime Parse(object value)
        {
            return ((DateTime) value).ToUniversalTime();
        }
    }
}