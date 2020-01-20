using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
namespace ApiProxyTest
{
    class Program
    {
        static CRL.Core.ApiProxy.ApiClientConnect clientConnect;
        static void Main(string[] args)
        {
            clientConnect = new CRL.Core.ApiProxy.ApiClientConnect("https://api.weixin.qq.com");

            //clientConnect.UseConsulDiscover("http://127.0.0.1:8500", "serviceName");//使用consul发现服务
            clientConnect.UseOcelotApiGateway("http://127.0.0.1:3400");//直接使用ocelot网关
            //clientConnect.UseConsulApiGatewayDiscover("http://127.0.0.1:3400", "serviceName");//使用ocelot网关发现服务

            clientConnect.UseBeforRequest((request, members, url) =>
            {
                //如果需要设置发送头信息
                request.SetHead("token", "test");
            });
            //https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=APPID&secret=APPSECRET
            var client = clientConnect.GetClient<IToken>();
            client.Test(new args()
            {
                name = "args",
                name2 = "sss",
                uArgs = new userInfo() { errcode = "111" }
            });

            //如果参数正确,返回token
            var result = client.token("grant_type", "appid", "secret");
            
            Console.WriteLine(result.ToJson());

            getInfo();

            Console.ReadLine();
        }

        static async void getInfo()
        {
            //https://api.weixin.qq.com/cgi-bin/user/info?access_token=ACCESS_TOKEN&openid=OPENID&lang=zh_CN
            var client2 = clientConnect.GetClient<IUser>();
            //如果参数正确,返回用户信息
            var result2 = await client2.info("222", "openid");//异步调用
            Console.WriteLine(result2.ToJson());
        }
    }

    /// <summary>
    /// 微信获取token
    /// </summary>
    [CRL.Core.ApiProxy.Service(ContentType = CRL.Core.ApiProxy.ContentType.JSON, GatewayPrefix = "clientservice")]
    public interface IToken
    {
        [CRL.Core.ApiProxy.Method(Path = "cgi-bin/token", Method = CRL.Core.ApiProxy.HttpMethod.GET)]
        Dictionary<string,string> token(string grant_type, string appid, string secret);

        [CRL.Core.ApiProxy.Method(Path = "cgi-bin/token/test", Method = CRL.Core.ApiProxy.HttpMethod.POST, ContentType = CRL.Core.ApiProxy.ContentType.FORM)]
        void Test(args args);
    }

    /// <summary>
    /// 微信获取用户信息
    /// </summary>
    public interface IUser
    {
        [CRL.Core.ApiProxy.Method(Path = "cgi-bin/user/info", Method = CRL.Core.ApiProxy.HttpMethod.GET)]
        Task<userInfo> info(string access_token, string openid, string lang = "zh_CN");
    }
    #region obj
    public class userInfo
    {
        public string errcode { get; set; }
        public string errmsg { get; set; }

        public string openid { get; set; }
        public string nickname { get; set; }
        public string headimgurl { get; set; }
        public args args { get; set; }
    }
    public class args
    {
        public string name
        {
            get;set;
        }
        public string name2
        {
            get; set;
        }

        public userInfo uArgs { get; set; }
    }
    #endregion
}
