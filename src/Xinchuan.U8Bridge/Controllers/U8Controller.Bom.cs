using System.Web.Http;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.Controllers
{
    public sealed partial class U8Controller
    {
        [HttpPost]
        [Route("bom/add")]
        public IHttpActionResult AddBom(BomAddRequest request)
        {
            return Invoke(request, "U8API/BOM/BomAdd", "bom-add", request?.ParentMaterialCode, true);
        }

        [HttpPost]
        [Route("bom/load")]
        public IHttpActionResult LoadBom(BomActionRequest request)
        {
            return Invoke(request, "U8API/BOM/BomLoad", "bom-load", BomKey(request), false);
        }

        [HttpPost]
        [Route("bom/part-lookup")]
        public IHttpActionResult LookupBomPart(BomPartLookupRequest request)
        {
            return Bridge(service.QueryBomParts(RequestIdHeader(), request));
        }

        [HttpPost]
        [Route("bom/load-by-code")]
        public IHttpActionResult LoadBomByCode(BomPartLookupRequest request)
        {
            return Bridge(service.LoadBomByCode(RequestIdHeader(), request));
        }

        [HttpPost]
        [Route("bom/tree-query")]
        public IHttpActionResult QueryBomTree(BomPartLookupRequest request)
        {
            return Bridge(service.QueryBomTree(RequestIdHeader(), request));
        }

        [HttpPost]
        [Route("bom/audit")]
        public IHttpActionResult AuditBom(BomActionRequest request)
        {
            return Invoke(request, "U8API/BOM/BomAuditing", "bom-audit", BomKey(request), false);
        }

        [HttpPost]
        [Route("bom/unaudit")]
        public IHttpActionResult UnauditBom(BomActionRequest request)
        {
            return Invoke(request, "U8API/BOM/BomUnauditing", "bom-unaudit", BomKey(request), false);
        }

        [HttpPost]
        [Route("bom/delete")]
        public IHttpActionResult DeleteBom(BomActionRequest request)
        {
            return Invoke(request, "U8API/BOM/BomDelete", "bom-delete", BomKey(request), false);
        }

        private static string BomKey(BomActionRequest request)
        {
            return request == null || !request.PartId.HasValue
                ? null
                : request.PartId.Value + ":" + request.BomType + ":" + request.VersionOrIdentCode;
        }
    }
}
