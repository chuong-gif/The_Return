/*
 * Mục đích: Quản lý toàn bộ lời hướng dẫn, tên ký hiệu và phản hồi của nhiệm vụ 5 trong Inspector.
 * Hàm: Symbol trả tên an toàn; Notebook soạn ghi chép riêng; Message đổi kết quả khóa thành lời nhắc.
 */
using System.Text;
using UnityEngine;
namespace TheReturn
{
    [CreateAssetMenu(menuName="The Return/Dont Look Back Catalog")]
    public sealed class DontLookBackCatalog : ScriptableObject
    {
        public string title="KHÔNG ĐƯỢC QUAY ĐẦU";
        [TextArea] public string introduction="Hướng an toàn theo mũi tên +Z. Đi hai làn, lùi để đọc lưng đồng đội bằng E. Đổi vị trí trước/sau, rồi tập hợp cuối hành lang và nhập ký hiệu theo số thẻ.";
        [TextArea] public string warning="Đang nhìn quá lệch! Nhìn theo mũi tên. Quá 110° trong 0,6 giây sẽ về checkpoint.";
        [TextArea] public string penalty="Có người quay đầu. Đội trở về checkpoint; ghi chép và mã đã nhập vẫn được giữ.";
        [TextArea] public string success="Đã xác nhận đủ ký hiệu. Cửa mở — cả đội có thể đi tiếp.";
        public string controls="WASD đi/lùi • E đọc thẻ/mở khóa • 1–4 đổi vai • R thử lại • Tab menu";
        public string[] symbolNames={"Tam giác","Tròn","Vuông","Chữ X","Dấu cộng","Hình thoi"};
        public string[] worldSymbols={"△","○","□","X","+","◇"};
        /// <summary>Nhận ID ký hiệu; trả tên trong catalog, dấu hỏi nếu cấu hình thiếu.</summary>
        public string Symbol(int id) => symbolNames!=null&&id>=0&&id<symbolNames.Length?symbolNames[id]:"?";
        /// <summary>Nhận trạng thái và vai; chỉ soạn ký hiệu vai này đã đọc của người khác.</summary>
        public string Notebook(DontLookBackState state,int actor)
        {
            var text=new StringBuilder("GHI CHÉP RIÊNG\n");
            for(int i=0;i<state.PlayerCount;i++)
                text.Append("Thẻ ").Append(i+1).Append(": ").Append(i==actor?"nhờ đồng đội đọc":state.HasSeen(actor,i)?Symbol(state.SymbolOf(i)):"chưa quan sát").Append("\n");
            return text.Append("\nĐứng lùi hơn đồng đội, nhìn vào lưng họ và nhấn E. Được phép đi lùi; không quay nhìn phía sau.").ToString();
        }
        /// <summary>Nhận kết quả khóa; trả lời nhắc, không đổi trạng thái.</summary>
        public string Message(BackLockResult result)
        {
            switch(result)
            {
                case BackLockResult.Solved:return success;
                case BackLockResult.NeedParty:return "Cần cả đội đứng trong vùng cuối hành lang.";
                case BackLockResult.NeedWitnesses:return "Mỗi thẻ cần được một đồng đội đọc bằng E trước khi mở khóa.";
                case BackLockResult.WrongCode:return "Mã chưa đúng. Đối chiếu số thẻ, không dùng thứ tự đến cửa.";
                default:return "Hãy đứng gần bảng khóa để thao tác.";
            }
        }
    }
}
