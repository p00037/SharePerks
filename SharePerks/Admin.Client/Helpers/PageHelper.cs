using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Admin.Client.Helpers
{
    public class PageHelper
    {
        public static string DisplayNameFor<T>(Expression<Func<T>> accessor)
        {
            // () => model.Prop 形式を想定（UnaryExpression になるケースも吸収）
            var body = accessor.Body is UnaryExpression u ? u.Operand : accessor.Body;

            if (body is not MemberExpression member || member.Member is not PropertyInfo prop)
                return string.Empty;

            // [Display(Name="...")] を取得。なければプロパティ名を返す
            var display = prop.GetCustomAttribute<DisplayAttribute>();
            return display?.GetName() ?? prop.Name;
        }
    }
}
