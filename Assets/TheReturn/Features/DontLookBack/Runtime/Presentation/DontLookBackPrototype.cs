/*
 * Mục đích: Nối luật không quay đầu với nhân vật, thẻ lưng, checkpoint, hai bảng khóa và cửa.
 * Hàm: Begin khởi tạo; StopSession kết thúc; Retry thử lại; ReturnToCheckpoint đặt đội;
 * Angle đo hướng; CanRead / Read xác thực quan sát; CanReach / TryOpen / ClosePanel quản lý khóa;
 * Select / Submit nhận thao tác UI; Update điều khiển/góc cấm/checkpoint; RefreshBadges cập nhật thẻ.
 */
using UnityEngine;
using UnityEngine.InputSystem;
namespace TheReturn
{
    [DefaultExecutionOrder(80)]
    public sealed class DontLookBackPrototype : MonoBehaviour
    {
        public PrototypePartyController party;
        public DontLookBackCatalog catalog;
        public Transform[] checkpoints, laterCheckpoints, terminals;
        public Transform[] badges;
        public TextMesh[] backLabels;
        public BoxCollider entryZone, checkpointZone, exitZone;
        public PuzzleDoor exitDoor;
        [Tooltip("0: đề mới mỗi phiên; số khác 0: seed cố định để tái hiện lỗi.")]
        public int initialSeed;
        public DontLookBackState State {get;private set;}
        public bool Active {get;private set;}
        public bool PanelOpen {get;private set;}
        public bool Armed {get;private set;}
        public int Checkpoint {get;private set;}
        public string Status {get;private set;}
        public int ReadTarget {get;private set;}=-1;
        public bool LockTarget {get;private set;}
        public bool Warning => Active&&Armed&&!State.Solved&&Angle(party.ActivePlayer)>=80;
        public float Blackout {get;private set;}
        int panelActor;

        /// <summary>Không nhận tham số; tạo đề theo đội/seed (0 sinh mới), chưa áp luật cho tới khi cả đội vào vùng đầu hành lang.</summary>
        public void Begin()
        {
            int seed=initialSeed==0?UnityEngine.Random.Range(1,int.MaxValue):initialSeed;
            State=new DontLookBackState(party.PartySize,seed);
            Active=true;PanelOpen=false;Armed=false;Checkpoint=0;Blackout=0;
            Status=catalog.introduction;exitDoor.SetOpen(false,true);RefreshBadges();
        }
        /// <summary>Nhận cờ reset cửa mặc định true; ngừng điều khiển và ẩn thẻ, giữ kết quả nếu chuyển nhiệm vụ.</summary>
        public void StopSession(bool reset=true)
        {
            Active=PanelOpen=Armed=false;Blackout=0;ReadTarget=-1;LockTarget=false;
            if(reset){State=null;exitDoor.SetOpen(false,true);}
            foreach(var badge in badges)badge.gameObject.SetActive(false);
        }
        /// <summary>Nhận cờ đổi đề; đặt lại nhiệm vụ ở đầu hành lang, không thay kết quả bốn nhiệm vụ trước.</summary>
        public void Retry(bool newCase=false)
        {
            if(!Active)return;
            int seed=newCase?unchecked(State.Seed+1):State.Seed;
            State=new DontLookBackState(party.PartySize,seed);
            Armed=true;Checkpoint=0;Blackout=0;PanelOpen=false;
            exitDoor.SetOpen(false,true);ReturnToCheckpoint();RefreshBadges();Status=catalog.introduction;
        }
        /// <summary>Không nhận tham số; đặt đội về checkpoint theo làn, hướng +Z; giữ ghi chép và lựa chọn khóa.</summary>
        public void ReturnToCheckpoint()
        {
            var points=Checkpoint==0?checkpoints:laterCheckpoints;
            for(int i=0;i<party.PartySize;i++)party.Teleport(i,points[i].position);
            party.SwitchRole(party.ActivePlayer,false);State.ClearExposure();
            PrototypePartyController.SetCursor(true);
        }
        /// <summary>Nhận vai; trả góc ngang tuyệt đối với +Z, không tính nhìn lên/xuống.</summary>
        public float Angle(int actor) => Vector3.Angle(Vector3.forward,party.players[actor].forward);
        /// <summary>Nhận chủ thẻ; kiểm tra người khác, khoảng cách, phía lưng, hướng ngắm và vật cản.</summary>
        public bool CanRead(int subject)
        {
            if(!Active||!Armed||State.Solved||subject<0||subject>=party.PartySize||subject==party.ActivePlayer)return false;
            Transform other=party.players[subject];
            Vector3 eye=party.viewCamera.transform.position;
            Vector3 target=other.TransformPoint(new Vector3(0,1.35f,-.38f));
            Vector3 delta=target-eye;
            if(delta.magnitude>6.5f||Vector3.Dot(other.forward,(eye-other.position).normalized)>-.25f||
                Vector3.Angle(party.viewCamera.transform.forward,delta)>20||Angle(party.ActivePlayer)>80)return false;
            foreach(var hit in Physics.RaycastAll(eye,delta.normalized,delta.magnitude,~0,QueryTriggerInteraction.Ignore))
                if(!hit.transform.IsChildOf(party.players[party.ActivePlayer])&&!hit.transform.IsChildOf(other))return false;
            return true;
        }
        /// <summary>Nhận chủ thẻ; chỉ ghi dữ liệu nếu thật sự nhìn thấy lưng, trả kết quả và phản hồi riêng.</summary>
        public bool Read(int subject)
        {
            if(!CanRead(subject)||!State.Observe(party.ActivePlayer,subject))return false;
            Status="Đã ghi thẻ "+(subject+1)+": "+catalog.Symbol(State.SymbolOf(subject))+". Hãy báo cho đồng đội.";
            return true;
        }
        /// <summary>Không nhận tham số; trả true nếu vai hiện tại gần một bảng khóa, nhìn đúng hướng và không bị tường chắn.</summary>
        public bool CanReach()
        {
            if(!Active||!Armed||Blackout>0||Angle(party.ActivePlayer)>80)return false;
            Vector3 eye=party.viewCamera.transform.position;
            foreach(var terminal in terminals)
            {
                Vector3 delta=terminal.position-eye;
                if(delta.magnitude>3.2f)continue;
                bool blocked=false;
                foreach(var hit in Physics.RaycastAll(eye,delta.normalized,delta.magnitude,~0,QueryTriggerInteraction.Ignore))
                    if(!hit.transform.IsChildOf(party.players[party.ActivePlayer])&&!hit.transform.IsChildOf(terminal))blocked=true;
                if(!blocked)return true;
            }
            return false;
        }
        /// <summary>Không nhận tham số; mở Canvas khi đủ tầm, ghi vai thao tác và trả thành công.</summary>
        public bool TryOpen()
        {
            if(!CanReach())return false;
            panelActor=party.ActivePlayer;PanelOpen=true;PrototypePartyController.SetCursor(false);return true;
        }
        /// <summary>Không nhận tham số; đóng bảng và trả chuột cho nhân vật.</summary>
        public void ClosePanel() { PanelOpen=false;PrototypePartyController.SetCursor(true); }
        /// <summary>Nhận ô/ký hiệu từ Canvas; xác thực vai và khoảng cách rồi chuyển tới luật.</summary>
        public bool Select(int slot,int symbol) => PanelOpen&&panelActor==party.ActivePlayer&&CanReach()&&State.Select(slot,symbol);
        /// <summary>Không nhận tham số; xác thực cả đội trong vùng cuối và mã, trả kết quả, mở cửa khi đúng.</summary>
        public BackLockResult Submit()
        {
            if(!PanelOpen||panelActor!=party.ActivePlayer||!CanReach())return BackLockResult.Invalid;
            bool gathered=true;
            for(int i=0;i<party.PartySize;i++)if(!exitZone.bounds.Contains(party.players[i].position))gathered=false;
            var result=State.Submit(gathered);Status=catalog.Message(result);
            if(State.Solved)exitDoor.SetOpen(true);
            return result;
        }
        /// <summary>Không nhận tham số; xử lý vai/input, vùng vào, checkpoint và thời gian vi phạm; menu không tích thời gian.</summary>
        void Update()
        {
            if(!Active||State==null)return;
            RefreshBadges();
            if(Blackout>0){Blackout=Mathf.Max(0,Blackout-Time.deltaTime);return;}
            var keyboard=Keyboard.current;
            if(keyboard!=null)
            {
                for(int i=0;i<party.PartySize;i++)
                    if(keyboard[(Key)((int)Key.Digit1+i)].wasPressedThisFrame)
                    {
                        bool reopen=PanelOpen;PanelOpen=false;party.SwitchRole(i,false);
                        Status=catalog.introduction;RefreshBadges();
                        if(reopen){PrototypePartyController.SetCursor(true);TryOpen();}
                    }
                if(keyboard.tabKey.wasPressedThisFrame||keyboard.escapeKey.wasPressedThisFrame)
                {
                    if(PanelOpen)ClosePanel();
                    else PrototypePartyController.SetCursor(Cursor.lockState!=CursorLockMode.Locked);
                }
                if(PanelOpen&&keyboard.eKey.wasPressedThisFrame){ClosePanel();return;}
            }
            if(PanelOpen)
            {
                if(panelActor!=party.ActivePlayer||!CanReach())ClosePanel();
                return;
            }
            if(Cursor.lockState!=CursorLockMode.Locked)return;
            if(keyboard!=null&&keyboard.rKey.wasPressedThisFrame){Retry();return;}
            party.TickInput(false);
            if(!Armed)
            {
                int gathered=0;
                for(int i=0;i<party.PartySize;i++)if(entryZone.bounds.Contains(party.players[i].position))gathered++;
                Status="Tập hợp vùng đầu hành lang: "+gathered+"/"+party.PartySize+". Luật bắt đầu khi đủ đội.";
                if(gathered==party.PartySize){Armed=true;ReturnToCheckpoint();Status=catalog.introduction;}
                return;
            }
            if(!State.Solved)
            {
                for(int i=0;i<party.PartySize;i++)
                    if(State.SampleLook(i,Angle(i),Time.deltaTime))
                    {
                        ReturnToCheckpoint();Blackout=.45f;Status=catalog.penalty;return;
                    }
                if(Checkpoint==0)
                {
                    bool together=true;
                    for(int i=0;i<party.PartySize;i++)if(!checkpointZone.bounds.Contains(party.players[i].position))together=false;
                    if(together){Checkpoint=1;Status="Đã lưu checkpoint 2. Ghi chép được giữ nếu quay đầu.";}
                }
            }
            ReadTarget=-1;
            for(int i=0;i<party.PartySize;i++)if(CanRead(i)){ReadTarget=i;break;}
            LockTarget=CanReach();
            if(keyboard!=null&&keyboard.eKey.wasPressedThisFrame)
            {
                if(ReadTarget>=0)Read(ReadTarget);else if(LockTarget)TryOpen();
            }
        }
        /// <summary>Không nhận tham số; chỉ hiện thẻ người khác trong phiên, để người chơi không tự xem lưng mình.</summary>
        void RefreshBadges()
        {
            for(int i=0;i<badges.Length;i++)
            {
                bool show=Active&&i<party.PartySize&&i!=party.ActivePlayer;
                badges[i].gameObject.SetActive(show);
                if(!show)continue;
                bool behind=Vector3.Dot(party.players[i].forward,(party.viewCamera.transform.position-party.players[i].position).normalized)<-.25f;
                backLabels[i].GetComponent<Renderer>().enabled=behind;
                badges[i].Find("FrontNumber").gameObject.SetActive(!behind);
                int symbol=State.SymbolOf(i);
                string mark=catalog.worldSymbols!=null&&symbol<catalog.worldSymbols.Length?catalog.worldSymbols[symbol]:"?";
                backLabels[i].text="THE "+(i+1)+"\n"+mark;
            }
        }
    }
}
