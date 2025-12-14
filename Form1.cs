using Microsoft.Win32;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace jfglzs_password_tool_v2
{

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            label1.Visible = false;
            string computerName = Environment.MachineName;
            // 获取最后一个字符的ASCII码
            byte asciiValue = (byte)computerName[computerName.Length - 1];
            // 显示ASCII码值
            textBox1.Text = asciiValue.ToString();
            radioButton1.Checked = true;
            Random random = new Random();
            string title = "";
            this.Text = GenerateTitle();

        }

        static string GenerateTitle()
        {
            Random rand = new Random();
            // 生成随机数
            int r1 = rand.Next(1, 992001);
            int r2 = rand.Next(1, 992001);
            int r3 = rand.Next(1, 992001);

            // 颜色数（简化处理）
            int colorCount = 256; // 或 rand.Next(16, 65537);
            int r4 = rand.Next(colorCount, 100001);
            int r5 = rand.Next(91, 1146);

            // 计算
            int sum = r1 + r2;
            int product = r4 * r5;



            // 构建标题
            return $"{sum} 机房-管理-助手_密码{r3}工具 {product}";
        }
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            DateTime currentTime = dateTimePicker1.Value;
            label1.Visible = true;
            if (radioButton1.Checked == true)
            {
                int temp = currentTime.Month * 159 + currentTime.Day * 357 + int.Parse(textBox1.Text) * 258;
                label1.Text = DecimalToBase7(temp);
            }
            if (radioButton2.Checked == true)
            {
                int temp = currentTime.Month * 123 + currentTime.Day * 456 + currentTime.Year * 789 + 111;
                label1.Text = DecimalToBase8(temp);
            }
            if (radioButton3.Checked == true)
            {
                int temp = currentTime.Month * 123 + currentTime.Day * 456 + currentTime.Year * 789 + 111;
                label1.Text = temp.ToString();
            }
            if (radioButton4.Checked == true)
            {
                int temp = (currentTime.Month * 13 + currentTime.Day * 57 + currentTime.Year * 91) * 16 + 11;
                string text1 = "8" + temp.ToString();
                label1.Text = text1;
            }
            if (radioButton5.Checked == true)
            {
                int temp = (currentTime.Month * 13 + currentTime.Day * 57 + currentTime.Year * 91) * 16;
                string text1 = "8" + temp.ToString();
                label1.Text = text1;
            }
        }
        static string DecimalToBase7(int decimalNumber)
        {
            if (decimalNumber == 0) return "0";

            bool isNegative = decimalNumber < 0;
            int number = Math.Abs(decimalNumber);
            string result = "";

            while (number > 0)
            {
                int remainder = number % 7;
                result = remainder.ToString() + result;
                number /= 7;
            }

            return isNegative ? "-" + result : result;
        }
        static string DecimalToBase8(int decimalNumber)
        {
            if (decimalNumber == 0) return "0";

            bool isNegative = decimalNumber < 0;
            int number = Math.Abs(decimalNumber);
            string octalResult = "";

            while (number > 0)
            {
                int remainder = number % 8;  // 获取余数
                octalResult = remainder.ToString() + octalResult;  // 添加到前面
                number /= 8;  // 除以8继续下一轮
            }

            return isNegative ? "-" + octalResult : octalResult;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            RegistryKey hklm = Registry.CurrentUser;
            RegistryKey hkSoftware = hklm.OpenSubKey("Software");




            if (hkSoftware != null)
            {
                textBox2.Text = (string)hkSoftware.GetValue("n");
                textBox3.Text = OptimizedTruncatedForgotIssuer.DecryptTruncatedOptimized(textBox2.Text);


            }
            else
            {
                MessageBox.Show("未找到注册表项！（可能没有安装助手）");
            }

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
    public class OptimizedTruncatedForgotIssuer
    {
        private const string WINDOWS_PATH = "C:\\WINDOWS";
        private const int TRUNCATE_COUNT = 2;

        // 优化的解密方法：先确定可能的字符范围
        public static string DecryptTruncatedOptimized(string truncatedEncrypted)
        {
            // 由于Base64 + 字符位移的特点，我们可以缩小搜索范围
            // Base64字符集: A-Z, a-z, 0-9, +, /, =
            // 经过-10的位移后，可能的字符范围

            // Base64字符的ASCII范围：'A'(65) 到 'z'(122)，加上'+','/','='
            // 减10后：55-112，还有一些特殊字符

            List<int> likelyFirstChars = new List<int>();
            List<int> likelyLastChars = new List<int>();

            // 确定可能的字符范围
            for (int c = 32; c <= 126; c++)
            {
                // 反向计算：如果这个字符是加密结果，那么原始Base64字符应该是c+10
                int originalChar = c + 10;

                // 检查originalChar是否在Base64字符范围内
                if (IsBase64Char(originalChar))
                {
                    likelyFirstChars.Add(c);
                    likelyLastChars.Add(c);
                }
            }

            Console.WriteLine($"优化搜索：从{(126 - 32 + 1) * (126 - 32 + 1)}种组合减少到{likelyFirstChars.Count * likelyLastChars.Count}种");

            // 穷举可能的组合
            foreach (int firstChar in likelyFirstChars)
            {
                foreach (int lastChar in likelyLastChars)
                {
                    try
                    {
                        string fullEncrypted = $"{(char)firstChar}{truncatedEncrypted}{(char)lastChar}";
                        string decrypted = DecryptFull(fullEncrypted);

                        if (IsLikelyPlaintext(decrypted))
                        {
                            return decrypted;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            // 如果优化搜索失败，回退到完整搜索
            return DecryptTruncatedFallback(truncatedEncrypted);
        }

        // 回退方法：完整搜索
        private static string DecryptTruncatedFallback(string truncatedEncrypted)
        {
            for (int firstChar = 32; firstChar <= 126; firstChar++)
            {
                for (int lastChar = 32; lastChar <= 126; lastChar++)
                {
                    try
                    {
                        string fullEncrypted = $"{(char)firstChar}{truncatedEncrypted}{(char)lastChar}";
                        string decrypted = DecryptFull(fullEncrypted);
                        return decrypted; // 第一个成功的就返回
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            throw new ArgumentException("解密失败");
        }

        // 检查是否为Base64字符
        private static bool IsBase64Char(int asciiCode)
        {
            char c = (char)asciiCode;
            return (c >= 'A' && c <= 'Z') ||
                   (c >= 'a' && c <= 'z') ||
                   (c >= '0' && c <= '9') ||
                   c == '+' || c == '/' || c == '=';
        }

        // 检查是否为可能的明文
        private static bool IsLikelyPlaintext(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length > 1000) // 假设明文不会太长
                return false;

            // 统计可打印字符比例
            int printableCount = 0;
            foreach (char c in text)
            {
                if (c >= 32 && c <= 126 || c == '\n' || c == '\r' || c == '\t')
                    printableCount++;
            }

            return (double)printableCount / text.Length > 0.95; // 95%以上可打印字符
        }

        // 完整解密
        private static string DecryptFull(string encryptedStr)
        {
            // 逆向字符位移
            string base64Str = ReverseRateIssuer(encryptedStr);

            // DES 解密
            string key = WINDOWS_PATH.Substring(0, 8);
            string iv = WINDOWS_PATH.Substring(1, 8);

            byte[] encryptedData = Convert.FromBase64String(base64Str);
            return DESDecrypt(encryptedData, key, iv);
        }

        private static string ReverseRateIssuer(string input)
        {
            char[] chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)(chars[i] + 10);
            }
            return new string(chars);
        }

        private static string DESDecrypt(byte[] encryptedData, string key, string iv)
        {
            using (DES des = DES.Create())
            {
                des.Key = Encoding.UTF8.GetBytes(key);
                des.IV = Encoding.UTF8.GetBytes(iv);
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = des.CreateDecryptor())
                {
                    byte[] decrypted = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                    return Encoding.UTF8.GetString(decrypted);
                }
            }
        }

    }
}

