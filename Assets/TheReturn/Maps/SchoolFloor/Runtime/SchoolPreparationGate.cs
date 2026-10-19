/*
 * Mục đích: cửa kiểm soát và vùng chuẩn bị của một lối vào phòng/hành lang.
 * Apply: nhận quyền mở cửa, cập nhật collider, visual và thông báo; không tự giải nhiệm vụ.
 * CountInside: nhận đội, đếm người trong hộp local để hỗ trợ cả vùng xoay.
 * CountOutside (T-016): đếm người ở khu chuẩn bị NGOÀI cửa, tức vùng đứng chờ để cả đội cùng mở cửa; mặc định là hộp phản chiếu của
 * preparationZone qua mặt phẳng cửa (không cần object mới), hoặc outsideZone nếu gán tay. Nhãn cửa: nhận thêm số người đang ở khu chuẩn bị.
 */
using UnityEngine;
namespace TheReturn
{
    public sealed class SchoolPreparationGate : MonoBehaviour
    {
        [Tooltip("0..5 theo thứ tự nhiệm vụ; -1 là khu chưa có nhiệm vụ (T-016: cửa đóng hẳn).")]
        public int mission = -1;
        public bool designatedEntrance;
        public BoxCollider blocker;
        public BoxCollider preparationZone;
        [Tooltip("Tùy chọn: vùng đứng chờ NGOÀI cửa. Để trống: tự tính bằng cách phản chiếu preparationZone qua mặt phẳng cửa (T-016).")]
        public BoxCollider outsideZone;
        public GameObject visualRoot;
        public TextMesh statusLabel;
        public string areaName;
        public bool IsOpen { get; private set; }

        /// <summary>Nhận quyền mở, trạng thái làm bài, đội, cờ khu đã xong, cờ khu có nhiệm vụ và số người đang ở khu chuẩn bị ngoài cửa (-1 = luồng cũ, không hiện); cập nhật chặn đường và nhãn, không trả dữ liệu.</summary>
        public void Apply(bool open,bool active,PrototypePartyController party,bool done=false,bool hasMission=false,int outsideGathered=-1)
        {
            IsOpen=open;
            blocker.enabled=!open;
            visualRoot.SetActive(!open);
            if(statusLabel)
            {
                bool mis=mission>=0 || hasMission;
                int size=party?party.PartySize:0;
                string state=!mis?"CHUA CO NHIEM VU":done?"DA HOAN THANH - LOI MO":open?(outsideGathered>=0?"LOI MO - CA DOI VAO TRONG":"LOI MO"):active?"HOAN THANH NHIEM VU DE MO CUA":outsideGathered>=0?"DUNG DU DOI O KHU CHUAN BI DE MO CUA":"CHO TIEN TRINH NHIEM VU";
                string extra="";
                if(mis && !active && !done)
                {
                    if(outsideGathered>=0 && !open)extra="\nKHU CHUAN BI: "+outsideGathered+"/"+size;
                    else if(open || designatedEntrance)extra="\nTAP HOP BEN TRONG: "+CountInside(party)+"/"+size;
                }
                statusLabel.text=areaName+"\n"+state+extra;
            }
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

        /// <summary>Nhận đội; trả số nhân vật đang hoạt động đứng ở khu chuẩn bị NGOÀI cửa (outsideZone nếu có, không thì hộp phản chiếu của preparationZone).</summary>
        public int CountOutside(PrototypePartyController party)
        {
            if(!party || !preparationZone)return 0;
            int count=0;
            for(int i=0;i<party.PartySize;i++)
                if(party.players[i].gameObject.activeInHierarchy && InOutside(party.players[i].position))count++;
            return count;
        }

        /// <summary>Nhận tọa độ thế giới; trả true nếu điểm nằm trong khu chuẩn bị ngoài cửa (phản chiếu qua mặt phẳng z=0 của cổng khi không có outsideZone).</summary>
        bool InOutside(Vector3 world)
        {
            if(outsideZone)
                return new Bounds(outsideZone.center,outsideZone.size).Contains(outsideZone.transform.InverseTransformPoint(world));
            Transform zone=preparationZone.transform;
            Vector3 centerWorld=zone.TransformPoint(preparationZone.center);
            Vector3 local=transform.InverseTransformPoint(centerWorld);
            Vector3 mirroredWorld=transform.TransformPoint(new Vector3(local.x,local.y,-local.z));
            Vector3 shifted=world+(centerWorld-mirroredWorld);
            return new Bounds(preparationZone.center,preparationZone.size).Contains(zone.InverseTransformPoint(shifted));
        }
    }
}
