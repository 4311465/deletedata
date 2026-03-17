using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace deletedata
{
    public static class TextBoxExtensions
    {
        public static bool TryParseDouble(this TextBox textBox, out double result, string fieldName = null)
        {
            result = 0;
            fieldName ??= textBox.Name;

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                ShowValidationError($"{fieldName}不能为空", textBox);
                return false;
            }

            if (!double.TryParse(textBox.Text.Trim(), out result))
            {
                ShowValidationError($"{fieldName}格式无效", textBox);
                return false;
            }

            return true;
        }

        public static bool TryParseDouble(this TextBox textBox, out double result,
            double minValue, double maxValue, string fieldName = null)
        {
            if (!textBox.TryParseDouble(out result, fieldName))
                return false;

            if (result < minValue || result > maxValue)
            {
                ShowValidationError($"{fieldName}必须在 {minValue} 到 {maxValue} 之间", textBox);
                return false;
            }

            return true;
        }

        private static void ShowValidationError(string message, TextBox textBox)
        {
            MessageBox.Show(message, "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            textBox.Focus();
            textBox.SelectAll();
        }
    }

// 使用方式（非常简洁）


}
