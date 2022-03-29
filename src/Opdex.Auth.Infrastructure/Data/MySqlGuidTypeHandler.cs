using System.Data;
using Dapper;

namespace Opdex.Auth.Infrastructure.Data;

public class MySqlGuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid value) => parameter.Value = value.ToString();

    public override Guid Parse(object value) => new((string) value);
}