/*
 * Mục đích: cửa kiểm soát và vùng chuẩn bị của một lối vào phòng/hành lang.
 * Apply: nhận quyền mở cửa, cập nhật collider, visual và thông báo; không tự giải nhiệm vụ.
 * CountInside: nhận đội, đếm người trong hộp local để hỗ trợ cả vùng xoay.
 */
using UnityEngine;
namespace TheReturn
{
    public sealed class SchoolPreparationGate : MonoBehaviour
    {
        [Tooltip("0..5 theo thứ tự nhiệm vụ; -1 là khu chưa có nhiệm vụ, luôn mở.")]
        public int mission = -1;
        public bool designatedEntrance;
        public BoxCollider blocker;
        public BoxCollider preparationZone;
        public GameObject visualRoot;
        public TextMesh statusLabel;
        public string areaName;
        public bool IsOpen { get; private set; }

        /// <summary>Nhận quyền mở, trạng thái làm bài, đội và cờ khu đã xong (mặc định false); cập nhật chặn đường và nhãn, không trả dữ liệu.</summary>
        public void Apply(bool open,bool active,PrototypePartyController party,bool done=false)
        {
            IsOpen=open;
            blocker.enabled=!open;
            visualRoot.SetActive(!open);
            if(statusLabel)
                statusLabel.text=areaName+"\n"+(mission<0?"CHUA CO NHIEM VU":done?"DA HOAN THANH - LOI MO":open?"LOI MO":active?"HOAN THANH NHIEM VU DE MO CUA":"CHO TIEN TRINH NHIEM VU")+
                    ((designatedEntrance || open) && !active && !done && mission>=0?"\nTAP HOP BEN TRONG: "+CountInside(party)+"/"+(party?party.PartySize:0):"");
        }

        /// <summary>Nhận đội; trả số nhân vật đang hoạt động nằm trong vùng chuẩn bị theo tọa độ local.</summary>
        public int CountInside(PrototypePartyController party)
        {
            if(!party || !preparationZone)return 0;
            int count=0;var box=new Bounds(preparationZone.center,preparationZone.size);
            for(int i=0;i<party.PartySize;i++)
                if(party.players[i].gameObject.activeInHierarchy && box.Contains(preparationZone.transform.InverseTransformPoint(party.players[i].position)))count++;
            return count;
        }
    }
}
