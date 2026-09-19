/*
 * Mục đích: Luật thuần C# cho ký hiệu lưng, ghi chép riêng, góc cấm và khóa đội 2–4.
 * Hàm: constructor sinh đề; SymbolOf / Entry / HasSeen / Witnessed đọc dữ liệu;
 * Observe ghi nhận nhân chứng; Select chọn mã; Submit xác thực cả đội và mã;
 * SampleLook tích thời gian sai hướng; ClearExposure xóa bộ đếm khi về checkpoint.
 */
using System;
namespace TheReturn
{
    public enum BackLockResult { Invalid, NeedWitnesses, NeedParty, WrongCode, Solved }
    public sealed class DontLookBackState
    {
        public const int SymbolCount = 6;
        public int PlayerCount { get; }
        public int Seed { get; }
        public bool Solved { get; private set; }
        public int Penalties { get; private set; }
        public int WitnessedCount
        {
            get { int count=0; for(int i=0;i<PlayerCount;i++) if(Witnessed(i))count++; return count; }
        }
        readonly int[] symbols, entries;
        readonly bool[,] seen;
        readonly float[] exposure;

        /// <summary>Nhận đội 2–4 và seed; sinh ký hiệu không trùng, không trả đối tượng có dữ liệu ngoài giới hạn.</summary>
        public DontLookBackState(int count,int seed)
        {
            if(count<2||count>4)throw new ArgumentOutOfRangeException(nameof(count));
            PlayerCount=count;Seed=seed;
            symbols=new int[count];entries=new int[count];seen=new bool[count,count];exposure=new float[count];
            int[] pool={0,1,2,3,4,5};
            var random=new Random(seed);
            for(int i=pool.Length-1;i>0;i--) { int j=random.Next(i+1); int t=pool[i];pool[i]=pool[j];pool[j]=t; }
            for(int i=0;i<count;i++){symbols[i]=pool[i];entries[i]=-1;}
        }
        /// <summary>Nhận ID vai; trả ký hiệu nội bộ cho lớp hiển thị lưng, -1 nếu sai ID.</summary>
        public int SymbolOf(int actor) => actor>=0&&actor<PlayerCount?symbols[actor]:-1;
        /// <summary>Nhận ID ô nhập; trả lựa chọn hoặc -1 nếu chưa chọn/sai ID.</summary>
        public int Entry(int actor) => actor>=0&&actor<PlayerCount?entries[actor]:-1;
        /// <summary>Nhận người đọc và chủ thẻ; trả dữ liệu đã ghi riêng của người đọc.</summary>
        public bool HasSeen(int observer,int subject) => observer>=0&&observer<PlayerCount&&subject>=0&&subject<PlayerCount&&seen[observer,subject];
        /// <summary>Nhận chủ thẻ; trả true nếu một người khác đã quan sát thẻ này.</summary>
        bool Witnessed(int subject)
        {
            for(int i=0;i<PlayerCount;i++)if(seen[i,subject])return true;
            return false;
        }
        /// <summary>Nhận hai vai đã được lớp scene kiểm tra tầm nhìn; ghi nhận, không cho tự đọc lưng mình.</summary>
        public bool Observe(int observer,int subject)
        {
            if(observer<0||observer>=PlayerCount||subject<0||subject>=PlayerCount||observer==subject||Solved)return false;
            seen[observer,subject]=true;return true;
        }
        /// <summary>Nhận ô và ký hiệu; trả false với đầu vào sai/đã giải, còn lại lưu lựa chọn.</summary>
        public bool Select(int slot,int symbol)
        {
            if(Solved||slot<0||slot>=PlayerCount||symbol<0||symbol>=SymbolCount)return false;
            entries[slot]=symbol;return true;
        }
        /// <summary>Nhận trạng thái tập hợp từ scene; trả lý do thất bại hoặc mở khóa, không phạt mã sai.</summary>
        public BackLockResult Submit(bool allAtExit)
        {
            if(Solved)return BackLockResult.Solved;
            if(!allAtExit)return BackLockResult.NeedParty;
            if(WitnessedCount<PlayerCount)return BackLockResult.NeedWitnesses;
            for(int i=0;i<PlayerCount;i++)if(entries[i]!=symbols[i])return BackLockResult.WrongCode;
            Solved=true;ClearExposure();return BackLockResult.Solved;
        }
        /// <summary>Nhận vai, góc ngang và thời gian hợp lệ; trả true tại lần vi phạm 110° liên tục đủ 0,6 giây.</summary>
        public bool SampleLook(int actor,float angle,float seconds)
        {
            if(Solved||actor<0||actor>=PlayerCount||float.IsNaN(angle)||float.IsInfinity(angle)||
                float.IsNaN(seconds)||float.IsInfinity(seconds)||seconds<=0)return false;
            exposure[actor]=Math.Abs(angle)>110?exposure[actor]+seconds:0;
            if(exposure[actor]<.6f)return false;
            Penalties++;ClearExposure();return true;
        }
        /// <summary>Không nhận tham số; xóa thời gian nguy hiểm, giữ đề, ghi chép và mã đang nhập.</summary>
        public void ClearExposure() { Array.Clear(exposure,0,exposure.Length); }
    }
}
