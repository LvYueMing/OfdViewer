using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Models.Signature
{
    /// <summary>
    /// 签名节点的类型,目前规定了两个可选值,
    /// Seal 表示是安全签章,
    /// Sign 表示是纯数字签名
    /// </summary>
    public enum SignatureType
    {
        Seal,
        Sign
    }
}
