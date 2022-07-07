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
        public static async Task<IFlurlResponse> GetMultipartData(string address,string imagefile)
        {
            var resp = await address.WithHeaders(new {User_Agent="Predictorian"}).PostMultipartAsync(mp => mp
                 .AddFile("file", imagefile));
            return resp;
        }

    }
}
