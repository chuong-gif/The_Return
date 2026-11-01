/*
 * Mục đích: Những điều HOST quan sát được từ thế giới vật lý mà luật thuần không biết (T-027): ô nào đứng gần thẻ / cần gạt, ô nào đang đứng
 * trên ván, thẻ thả xuống có nằm trên ván không. Lớp Unity trên host cài đặt từ vị trí nhân vật; test cài đặt giả. Nhờ vậy luật chạy
 * và test được mà không cần scene, và client không tự khai vị trí của mình.
 * Hàm: IsNearCard; IsNearLever; PlayersOnDeck; DropSpot (kèm kiểu PlankDropSpot).
 */
namespace TheReturn.PlankNet
{
    /// <summary>Chỗ thẻ từ rơi xuống khi được thả: nằm trên ván hay trên sàn, và toạ độ theo làn (X ngang, Z dọc hành lang).</summary>
    public readonly struct PlankDropSpot
    {
        /// <summary>True nếu thẻ rơi vào vùng ván.</summary>
        public bool OnDeck { get; }
        /// <summary>Toạ độ X trong hệ làn: nếu OnDeck thì lệch so với tâm ván, ngược lại là toạ độ tuyệt đối trên sàn.</summary>
        public float LocalX { get; }
        /// <summary>Toạ độ Z trong hệ làn, cùng quy ước như LocalX.</summary>
        public float LocalZ { get; }

        /// <summary>Nhận cờ trên ván và hai toạ độ làn.</summary>
        public PlankDropSpot(bool onDeck, float localX, float localZ)
        {
            OnDeck = onDeck;
            LocalX = localX;
            LocalZ = localZ;
        }
    }

    /// <summary>Nguồn sự thật vật lý của nhiệm vụ cầu ván, do host cung cấp.</summary>
    public interface IPlankWorld
    {
        /// <summary>Nhận ô; trả true nếu ô đó đứng đủ gần thẻ từ để nhặt.</summary>
        bool IsNearCard(int slot);

        /// <summary>Nhận ô và bờ của cần gạt (true là bờ A); trả true nếu ô đó đứng đủ gần cần đó.</summary>
        bool IsNearLever(int slot, bool sideA);

        /// <summary>Trả các ô đang đứng trong vùng ván (có thể nhiều hơn 2; luật tự kiểm).</summary>
        int[] PlayersOnDeck();

        /// <summary>Nhận ô đang thả thẻ; trả chỗ thẻ rơi xuống (trên ván hay trên sàn, kèm toạ độ làn) để mọi máy vẽ thẻ giống nhau.</summary>
        PlankDropSpot DropSpot(int slot);
    }
}
