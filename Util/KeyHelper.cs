using System;
using System.Collections.Generic;
using System.Text;

namespace Util
{
    public static class KeyHelper
    {
        /// <summary>
        /// 生成主键
        /// </summary>
        /// <returns></returns>
        public static string GenerateKey()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
