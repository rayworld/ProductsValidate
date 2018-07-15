using System.Web.Http;
using System.Web.Mvc;
using Ray.Framework.Encrypt;
using Ray.Framework.DBUtility;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Http;
using ProductsValidate.Models;

namespace ProductsValidate.Controllers
{
    public class ValiController : ApiController
    {
        //POST: api/check
        [System.Web.Http.HttpPost]
        public HttpResponseMessage CheckProdInfo([FromBody]ProductInfo productinfo)
        {
            string errInfo = "";
            //翻译明码
            string mingQRCode = EncryptHelper.Decrypt(productinfo.QRCode);

            ///////////////////////////
            ///唯一码是否有效
            ///////////////////////////
            if (IsNumber(mingQRCode) == false)
            {
                errInfo = "{\"Code\":\"" + "3" + "\",\"Result\":\"" + RetVal.唯一码错误.ToString() + "\"}";
            }

            ///////////////////////////
            ///特殊码，直接通过
            ///////////////////////////
            else if (int.Parse(mingQRCode) <= 169021999 && int.Parse(mingQRCode) >= 169000000)
            {
                errInfo = "{\"Code\":\"" + "0" + "\",\"Result\":\"" + RetVal.验证通过.ToString() + "\"}";
            }

            ///////////////////////////
            ///唯一码是否存在
            ///////////////////////////
            else if (Exist(mingQRCode) == false)
            {
                errInfo = "{\"Code\":\"" + "5" + "\",\"Result\":\"" + RetVal.唯一码不存在.ToString() + "\"}";
            }



            /////////////////////////////////////
            //客户号是否匹配
            /////////////////////////////////////
            else if (MatchCustomId(mingQRCode, productinfo.CustomId) == false)
            {
                if (ExistCustomId(productinfo.CustomId) == true)
                {
                    errInfo = "{\"Code\":\"" + "1" + "\",\"Result\":\"" + RetVal.客户编号错误.ToString() + "\"}";

                }
                else
                {
                    errInfo = "{\"Code\":\"" + "6" + "\",\"Result\":\"" + RetVal.客户编号不存在.ToString() + "\"}";
                }
            }
            /////////////////////////////////////
            //产品号是否匹配
            /////////////////////////////////////
            else if (MatchProductId(mingQRCode, productinfo.ProductId) == false)
            {
                errInfo = "{\"Code\":\"" + "2" + "\",\"Result\":\"" + RetVal.产品编号错误.ToString() + "\"}";
            }
            //////////////////////////////////
            ///是否审核过
            //////////////////////////////////
            else if (Checked(mingQRCode) == true)
            {
                errInfo = "{\"Code\":\"" + "4" + "\",\"Result\":\"" + RetVal.已经核销过.ToString() + "\"}";
            }
            else
            {
                errInfo = "{\"Code\":\"" + "0" + "\",\"Result\":\"" + RetVal.验证通过.ToString() + "\"}";
            }
            HttpResponseMessage result = new HttpResponseMessage { Content = new StringContent(errInfo, Encoding.GetEncoding("UTF-8"), "application/json") };
            return result;
        }

        #region enum

        public enum RetVal
        {
            验证通过,
            客户编号错误,
            客户编号不存在,
            产品编号错误,
            唯一码错误,
            唯一码不存在,
            已经核销过,
        }

        #endregion

        #region function

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool IsNumber(string str)
        {
            string s = @"^\d+$";
            Regex regex = new Regex(s);
            return regex.IsMatch(str) ? true : false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="qrcode"></param>
        /// <returns></returns>
        private bool Exist(string qrcode)
        {
            string tableName = qrcode.Substring(0, 4);
            string sql = string.Format("SELECT COUNT(*) FROM t_QRCode{0} WHERE FQRCode = {1}", tableName, qrcode);
            object obj = SqlHelper.ExecuteScalar(sql);
            return obj != null && int.Parse(obj.ToString()) > 0 ? true : false;
        }

        private bool MatchCustomId(string qrcode, string customId)
        {
            string tableName = qrcode.Substring(0, 4);
            //string sql = string.Format("SELECT [客户编号] FROM [dbo].[icstock] WHERE [单据编号] In (SELECT SUBSTRING(FEntryID,0,11) FROM [dbo].[t_QRCode{0}] WHERE FQRCode = {1}) and [FEntryID] In (SELECT SUBSTRING(FEntryID,12,4) FROM [dbo].[t_QRCode{0}] WHERE FQRCode = {1})", tableName, qrcode);
            string sql = string.Format("SELECT [客户编号] FROM [dbo].[icstock] WHERE [单据编号] In (SELECT TOP 1  SUBSTRING(FEntryID,0,11) FROM [dbo].[t_QRCode{0}] WHERE FQRCode = '{1}' order by FCodeID desc ) and [FEntryID] In (SELECT  TOP 1 SUBSTRING(FEntryID,12,4) FROM [dbo].[t_QRCode{0}] WHERE FQRCode = '{1}' order by FCodeID desc)", tableName, qrcode);
            object obj = SqlHelper.ExecuteScalar(sql);
            if (obj != null)
            {
                string custId = obj.ToString();
                return custId.Substring(0, custId.Length - 4) == customId.Substring(0, customId.Length - 4) ? true : false;
            }
            else
            {
                return false;
            }
        }

        private bool MatchProductId(string qrcode, string productId)
        {
            string tableName = qrcode.Substring(0, 4);
            //string sql = string.Format("SELECT [产品编号] FROM [dbo].[icstock] WHERE [单据编号] In (SELECT  SUBSTRING(FEntryID,0,11) FROM [dbo].[t_QRCode{0}] WHERE FQRCode = {1}) and [FEntryID] In (SELECT SUBSTRING(FEntryID,12,4) FROM [dbo].[t_QRCode{0}] WHERE FQRCode = {1})", tableName, qrcode);
            string sql = string.Format("SELECT [产品编号] FROM [dbo].[icstock] WHERE [单据编号] In (SELECT Top 1  SUBSTRING(FEntryID,0,11) FROM [dbo].[t_QRCode{0}] WHERE FQRCode = '{1}'  order by FCodeID desc) and [FEntryID] In (SELECT Top 1 SUBSTRING(FEntryID,12,4) FROM [dbo].[t_QRCode{0}] WHERE FQRCode = '{1}'  order by FCodeID desc)", tableName, qrcode);
            object obj = SqlHelper.ExecuteScalar(sql);
            //return obj != null && obj.ToString() == productId ? true : false;
            if (obj != null)
            {
                string prodId = obj.ToString();
                return prodId.Substring(0, prodId.Length - 5) == productId ? true : false;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="qrcode"></param>
        /// <returns></returns>
        private bool Checked(string qrcode)
        {
            string tableName = "";
            string sql = "";
            tableName = qrcode.Substring(0, 4);
            sql = string.Format("SELECT FQRCode FROM t_QRCode{0} WHERE FQRCode = {1} AND FState = 'C'", tableName, qrcode);
            object obj = SqlHelper.ExecuteScalar(sql);
            return obj != null ? true : false;
        }

        private bool ExistCustomId(string customId)
        {
            string sql = string.Format("select  distinct [客户编号] from [dbo].[icstock] where [客户编号] = '{0}'", customId);
            object obj = SqlHelper.ExecuteScalar(sql);
            return obj != null && obj.ToString() == customId ? true : false;
        }
        #endregion
    }
}
