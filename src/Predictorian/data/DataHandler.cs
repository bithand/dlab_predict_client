using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Predictorian.data
{
    public class DataHandler
    {
        public static async Task<IFlurlResponse> GetMultipartData(string address, string imagefile, string authname = "asbestos", string password = "yXbjry6aHkf2EvtHZk46mMFno")
        {
            var resp = await address.WithHeaders(new {User_Agent="Predictorian"}).WithBasicAuth(authname,password).PostMultipartAsync(mp => mp
                 .AddFile("file", imagefile));
            return resp;
        }

    }
}
