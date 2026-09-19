/*
 * Mục đích: Các tham chiếu Canvas được gán sẵn trong prefab; không chứa luật gameplay.
 * Hàm: SetText đổi chữ chỉ khi cần; SetVisible đổi trạng thái panel khi có thay đổi.
 */
using UnityEngine;
using UnityEngine.UI;
namespace TheReturn
{
    public sealed class SchoolFloorCanvasView : MonoBehaviour
    {
        public GameObject hud, pause, tests, gradeBoard, document, examBoard, cluePanel, noisePanel;
        public Text title, role, objective, status, prompt, controls, clues, documentTitle, documentBody, testLabel;
        public Slider noise;
        public Button resume, retry, nextCase, restartMap, openTests, closeTests;
        public Button[] teamButtons, missionButtons, gradeVouchers, gradeTargets, gradeSources, directions;
        public Button gradeApply, gradeUndo, gradeSubmit, gradeClose, documentClose, examClose;
        public Text[] gradeRows, examCells;
        public Text gradeSummary, examSummary;
        public Image crosshair, blackout;
        public GameObject backBoard;
        public Button[] backSlots;
        public Button backSubmit, backClose;
        public Text backSummary;

        /// <summary>Nhận Text và nội dung; chỉ gán khi khác để hạn chế Canvas rebuild.</summary>
        public static void SetText(Text target,string value)
        {
            if(target!=null && target.text!=value) target.text=value??"";
        }

        /// <summary>Nhận panel và cờ; chỉ đổi active khi cần, không tác động luật game.</summary>
        public static void SetVisible(GameObject target,bool visible)
        {
            if(target!=null && target.activeSelf!=visible) target.SetActive(visible);
        }
    }
}
