using System.Web.Mvc;

public class ChatAIController : Controller
{
    [HttpPost]
    public JsonResult Ask(string message)
    {
        string reply = GetReply(message.ToLower().Trim());
        return Json(new { reply = reply }, JsonRequestBehavior.AllowGet);
    }

    private string GetReply(string msg)
    {
        // Chào hỏi
        if (msg.Contains("xin chào") || msg.Contains("hello") || msg.Contains("hi") || msg.Contains("chào"))
            return "Xin chào! Mình là trợ lý của Gas Spark 🥤 Mình có thể giúp gì cho bạn?";

        if (msg.Contains("bạn là ai") || msg.Contains("bot") || msg.Contains("ai vậy"))
            return "Mình là chatbot hỗ trợ của Gas Spark! Mình có thể tư vấn sản phẩm, đơn hàng, giao hàng cho bạn 😊";

        // Sản phẩm
        if (msg.Contains("sản phẩm") || msg.Contains("có gì") || msg.Contains("bán gì") || msg.Contains("menu"))
            return "Gas Spark có các sản phẩm: Nước ép Xoài, Nước ép Dâu Thạch, Trà Đào, Sữa Dừa Đá và nhiều loại khác. Bạn xem thêm tại mục Sản phẩm nhé!";

        if (msg.Contains("nước ép") || msg.Contains("nuoc ep"))
            return "Gas Spark có Nước ép Xoài (28.000đ) và Nước ép Dâu Thạch (34.000đ). Bạn muốn đặt loại nào?";

        if (msg.Contains("trà đào") || msg.Contains("tra dao"))
            return "Trà Đào Gas Spark giá 32.000đ, thơm ngon, thanh mát! Bạn có muốn thêm vào giỏ hàng không?";

        if (msg.Contains("sữa dừa") || msg.Contains("sua dua"))
            return "Sữa Dừa Đá là sản phẩm hot của Gas Spark! Bạn vào mục Sản phẩm để xem giá và đặt hàng nhé.";

        if (msg.Contains("giá") || msg.Contains("bao nhiêu") || msg.Contains("bao nhieu"))
            return "Sản phẩm của Gas Spark dao động từ 25.000đ - 45.000đ. Bạn xem chi tiết từng sản phẩm tại mục Danh sách sản phẩm nhé!";

        // Đơn hàng
        if (msg.Contains("đơn hàng") || msg.Contains("don hang") || msg.Contains("đặt hàng"))
            return "Bạn có thể xem lịch sử đơn hàng tại mục 'Đơn hàng' trên thanh menu. Cần hỗ trợ thêm liên hệ hotline: 0916 112 132";

        if (msg.Contains("hủy") || msg.Contains("huy don"))
            return "Để hủy đơn hàng, vui lòng liên hệ hotline 0916 112 132 trong vòng 30 phút sau khi đặt hàng nhé!";

        if (msg.Contains("thanh toán") || msg.Contains("thanh toan"))
            return "Gas Spark hỗ trợ thanh toán khi nhận hàng (COD). Bạn xem chi tiết tại mục Chính sách thanh toán nhé!";

        // Giao hàng
        if (msg.Contains("giao hàng") || msg.Contains("giao hang") || msg.Contains("ship"))
            return "Gas Spark giao hàng toàn quốc! Thời gian giao từ 2-5 ngày tùy khu vực. Xem chi tiết tại mục Chính sách giao hàng.";

        if (msg.Contains("phí ship") || msg.Contains("phí giao") || msg.Contains("phi ship"))
            return "Phí giao hàng tùy theo khu vực, thông thường từ 20.000đ - 40.000đ. Đơn hàng trên 200.000đ được miễn phí ship!";

        // Đổi trả
        if (msg.Contains("đổi trả") || msg.Contains("doi tra") || msg.Contains("trả hàng") || msg.Contains("hoàn tiền"))
            return "Gas Spark hỗ trợ đổi trả trong vòng 7 ngày nếu sản phẩm lỗi. Liên hệ hotline 0916 112 132 để được hỗ trợ!";

        // Liên hệ
        if (msg.Contains("hotline") || msg.Contains("điện thoại") || msg.Contains("liên hệ") || msg.Contains("lien he"))
            return "Hotline Gas Spark: 0916 112 132. Hỗ trợ từ 8:00 - 21:00 hàng ngày!";

        if (msg.Contains("địa chỉ") || msg.Contains("dia chi") || msg.Contains("ở đâu") || msg.Contains("o dau"))
            return "Bạn vui lòng liên hệ hotline 0916 112 132 để biết địa chỉ cửa hàng gần nhất nhé!";

        // Cảm ơn / Tạm biệt
        if (msg.Contains("cảm ơn") || msg.Contains("cam on") || msg.Contains("thanks"))
            return "Không có gì! Rất vui được hỗ trợ bạn 😊 Chúc bạn mua sắm vui vẻ tại Gas Spark!";

        if (msg.Contains("tạm biệt") || msg.Contains("tam biet") || msg.Contains("bye"))
            return "Tạm biệt! Hẹn gặp lại bạn tại Gas Spark 👋";

        // Mặc định
        return "Mình chưa hiểu câu hỏi của bạn 😅 Bạn có thể hỏi về: sản phẩm, giá cả, giao hàng, đổi trả, hoặc liên hệ hotline 0916 112 132 để được hỗ trợ trực tiếp nhé!";
    }
}