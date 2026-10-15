# Index script / symbol / nơi dùng (TỰ SINH — đừng sửa tay)

Sinh 2026-10-14 bằng execute_code (xem .ai/tools/rebuild_index.snippet.cs). Phạm vi: *.cs ngoài ThirdParty/TutorialInfo. Dùng-bởi = scene/prefab/asset YAML dưới Assets/TheReturn chứa GUID script (bỏ file Before*). Quét 49 file YAML. Bản đồ gần đúng: không thấy tham chiếu tạo bằng code (AddComponent, Find...). Source và Unity là nguồn sự thật.

| File | Dòng | Namespace | Kiểu | Dùng-bởi |
|---|---|---|---|---|
| Assets/Scripts/Player.cs | 17 |  | class Player | - |
| Art/Editor/ClassroomEnvironmentPass.cs | 244 | TheReturn.Art.Editor | class ClassroomEnvironmentPass | - |
| Art/Editor/MissionRoomEnvironmentPass.cs | 238 | TheReturn.Art.Editor | class MissionRoomEnvironmentPass | - |
| Art/Editor/SchoolCirculationBuilder.cs | 209 | TheReturn.Art.Editor | class SchoolCirculationBuilder | - |
| Art/Editor/SchoolInteriorArtPass.cs | 272 | TheReturn.Art.Editor | class SchoolInteriorArtPass | - |
| Art/Editor/SurrealArtPass.cs | 260 | TheReturn.Art.Editor | class SurrealArtPass | - |
| Art/Editor/UserBlockoutLayoutBuilder.cs | 198 | TheReturn.Art.Editor | class UserBlockoutLayoutBuilder | - |
| Core/Runtime/Players/PrototypePartyController.cs | 159 | TheReturn | class PrototypePartyController | 3: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity, Sanh_Cho.unity |
| Core/Runtime/SceneFlow/SceneTransitionService.cs | 82 | TheReturn | class SceneTransitionService | 1: Sanh_Cho.unity |
| Features/Attendance/Editor/AttendanceSceneBuilder.cs | 268 | TheReturn.Editor | class AttendanceSceneBuilder | - |
| Features/Attendance/Editor/AttendanceTextCatalogEditor.cs | 31 | TheReturn.Editor | class AttendanceTextCatalogEditor | - |
| Features/Attendance/Runtime/Data/AttendanceTextCatalog.cs | 155 | TheReturn | class AttendanceTextEntry, class AttendanceTextCatalog | 1: AttendanceText_VI.asset |
| Features/Attendance/Runtime/Domain/AttendanceGenerator.cs | 134 | TheReturn | class AttendanceGenerator | - |
| Features/Attendance/Runtime/Domain/AttendanceRound.cs | 105 | TheReturn | enum AttendanceRuleKind, class AttendanceRule, class AttendanceRound | - |
| Features/Attendance/Runtime/Domain/AttendanceState.cs | 88 | TheReturn | class AttendanceState | - |
| Features/Attendance/Runtime/Presentation/AttendanceHud.cs | 109 | TheReturn | class AttendanceHud | 2: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity |
| Features/Attendance/Runtime/Presentation/AttendancePrototype.cs | 331 | TheReturn | class AttendancePrototype | 2: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity |
| Features/Attendance/Runtime/Presentation/AttendanceSeat.cs | 40 | TheReturn | class AttendanceSeat | 3: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity, PF_AttendanceSeat.prefab |
| Features/Attendance/Tests/Editor/AttendanceChecks.cs | 111 | TheReturn.Editor | class AttendanceChecks | - |
| Features/DontLookBack/Runtime/Data/DontLookBackCatalog.cs | 44 | TheReturn | class DontLookBackCatalog | 1: DontLookBack_VI.asset |
| Features/DontLookBack/Runtime/Domain/DontLookBackState.cs | 83 | TheReturn | enum BackLockResult, class DontLookBackState | - |
| Features/DontLookBack/Runtime/Presentation/DontLookBackPrototype.cs | 205 | TheReturn | class DontLookBackPrototype | 2: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity |
| Features/GradeRepair/Runtime/Data/GradeRepairCatalog.cs | 71 | TheReturn | class GradeRepairCatalog | 1: GradeRepair_VI.asset |
| Features/GradeRepair/Runtime/Domain/GradeRepairRound.cs | 51 | TheReturn | class GradeRepairRound | - |
| Features/GradeRepair/Runtime/Domain/GradeRepairState.cs | 128 | TheReturn | enum GradeResult, class GradeRepairState | - |
| Features/GradeRepair/Runtime/Presentation/GradeRepairPrototype.cs | 190 | TheReturn | class GradeRepairPrototype | 2: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity |
| Features/GradeRepair/Runtime/Presentation/GradeRepairTerminal.cs | 17 | TheReturn | class GradeRepairTerminal | 3: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity, PF_GradeTerminal.prefab |
| Features/NavigationExam/Runtime/Data/NavigationExamCatalog.cs | 58 | TheReturn | class NavigationExamCatalog | 1: NavigationExam_VI.asset |
| Features/NavigationExam/Runtime/Domain/NavigationExamState.cs | 86 | TheReturn | enum ExamResult, class NavigationExamState | - |
| Features/NavigationExam/Runtime/Presentation/NavigationExamPrototype.cs | 151 | TheReturn | class NavigationExamPrototype | 2: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity |
| Features/QuietCorridor/Runtime/Data/QuietCorridorSettings.cs | 23 | TheReturn | class QuietCorridorSettings | 1: QuietCorridor_VI.asset |
| Features/QuietCorridor/Runtime/Domain/QuietCorridorState.cs | 85 | TheReturn | class QuietCorridorState | - |
| Features/QuietCorridor/Runtime/Presentation/QuietCorridorPrototype.cs | 167 | TheReturn | class QuietCorridorPrototype | 2: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity |
| Features/QuietCorridor/Runtime/Presentation/QuietHoldStation.cs | 27 | TheReturn | class QuietHoldStation | 3: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity, PF_QuietHoldStation.prefab |
| Features/QuietCorridor/Runtime/Presentation/QuietSpeakerFeedback.cs | 46 | TheReturn | class QuietSpeakerFeedback | 2: SchoolFloorBeforeUserBlockout.unity, PF_QuietSpeaker.prefab |
| Features/TeacherTruth/Runtime/Data/TeacherTruthCatalog.cs | 78 | TheReturn | class TeacherTruthCatalog | 1: TeacherTruth_VI.asset |
| Features/TeacherTruth/Runtime/Domain/TeacherTruthState.cs | 142 | TheReturn | enum ManualConfirmResult, class TeacherTruthState | - |
| Features/TeacherTruth/Runtime/Presentation/ManualConfirmStation.cs | 24 | TheReturn | class ManualConfirmStation | 3: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity, PF_ManualConfirmStation.prefab |
| Features/TeacherTruth/Runtime/Presentation/TeacherChannelSource.cs | 14 | TheReturn | class TeacherChannelSource | 3: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity, PF_TeacherChannel.prefab |
| Features/TeacherTruth/Runtime/Presentation/TeacherTruthPrototype.cs | 177 | TheReturn | class TeacherTruthPrototype | 2: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity |
| Maps/Lobby/Editor/LobbySceneBuilder.cs | 263 | TheReturn.Editor | class LobbySceneBuilder | - |
| Maps/Lobby/Runtime/LobbyCanvasView.cs | 50 | TheReturn | class LobbyCanvasView | 1: Sanh_Cho.unity |
| Maps/Lobby/Runtime/LobbyReadyController.cs | 155 | TheReturn | class LobbyReadyController | 1: Sanh_Cho.unity |
| Maps/SchoolFloor/Editor/DontLookBackSceneBuilder.cs | 169 | TheReturn.Editor | class DontLookBackSceneBuilder | - |
| Maps/SchoolFloor/Editor/GradeRepairSceneBuilder.cs | 174 | TheReturn.Editor | class GradeRepairSceneBuilder | - |
| Maps/SchoolFloor/Editor/GradeRepairSpatialPass.cs | 219 | TheReturn.Editor | class GradeRepairSpatialPass | - |
| Maps/SchoolFloor/Editor/MonumentalSchoolBuilder.cs | 221 | TheReturn.Editor | class MonumentalSchoolBuilder | - |
| Maps/SchoolFloor/Editor/NavigationExamSceneBuilder.cs | 162 | TheReturn.Editor | class NavigationExamSceneBuilder | - |
| Maps/SchoolFloor/Editor/QuietCorridorSceneBuilder.cs | 112 | TheReturn.Editor | class QuietCorridorSceneBuilder | - |
| Maps/SchoolFloor/Editor/SchoolFloorCanvasBuilder.DontLookBack.cs | 60 | TheReturn.Editor | class SchoolFloorCanvasBuilder | - |
| Maps/SchoolFloor/Editor/SchoolFloorCanvasBuilder.Panels.cs | 126 | TheReturn.Editor | class SchoolFloorCanvasBuilder | - |
| Maps/SchoolFloor/Editor/SchoolFloorCanvasBuilder.TeacherTruth.cs | 46 | TheReturn.Editor | class SchoolFloorCanvasBuilder | - |
| Maps/SchoolFloor/Editor/SchoolFloorCanvasBuilder.cs | 190 | TheReturn.Editor | class SchoolFloorCanvasBuilder | - |
| Maps/SchoolFloor/Editor/SchoolFloorObjectsWindow.cs | 76 | TheReturn.Editor | class SchoolFloorObjectsWindow | - |
| Maps/SchoolFloor/Editor/SchoolLayoutV2Migration.cs | 195 | TheReturn.Editor | class SchoolLayoutV2Migration | - |
| Maps/SchoolFloor/Editor/TeacherTruthSceneBuilder.cs | 205 | TheReturn.Editor | class TeacherTruthSceneBuilder | - |
| Maps/SchoolFloor/Runtime/GradeRepairHud.cs | 146 | TheReturn | class GradeRepairHud | 2: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity |
| Maps/SchoolFloor/Runtime/NavigationExamHud.cs | 112 | TheReturn | class NavigationExamHud | 2: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity |
| Maps/SchoolFloor/Runtime/QuietCorridorHud.cs | 82 | TheReturn | class QuietCorridorHud | 2: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity |
| Maps/SchoolFloor/Runtime/SchoolFloorAccessController.cs | 62 | TheReturn | class SchoolFloorAccessController | 1: SchoolFloorPrototype.unity |
| Maps/SchoolFloor/Runtime/SchoolFloorFlow.cs | 125 | TheReturn | class SchoolFloorFlow | 2: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity |
| Maps/SchoolFloor/Runtime/SchoolPreparationGate.cs | 43 | TheReturn | class SchoolPreparationGate | 2: SchoolFloorPrototype.unity, PF_PreparationGate.prefab |
| Maps/SchoolFloor/Runtime/Testing/MissionTestLauncher.cs | 143 | TheReturn | enum MissionTestEntry, class MissionTestLauncher | 2: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity |
| Maps/SchoolFloor/Runtime/UI/SchoolFloorCanvasPresenter.DontLookBack.cs | 52 | TheReturn | class SchoolFloorCanvasPresenter | - |
| Maps/SchoolFloor/Runtime/UI/SchoolFloorCanvasPresenter.Puzzles.cs | 63 | TheReturn | class SchoolFloorCanvasPresenter | - |
| Maps/SchoolFloor/Runtime/UI/SchoolFloorCanvasPresenter.cs | 225 | TheReturn | class SchoolFloorCanvasPresenter | 1: PF_SchoolFloorCanvas.prefab |
| Maps/SchoolFloor/Runtime/UI/SchoolFloorCanvasView.cs | 38 | TheReturn | class SchoolFloorCanvasView | 1: PF_SchoolFloorCanvas.prefab |
| Maps/SchoolFloor/Tests/Editor/DontLookBackChecks.cs | 142 | TheReturn.Editor | class DontLookBackChecks | - |
| Maps/SchoolFloor/Tests/Editor/GradeRepairChecks.cs | 214 | TheReturn.Editor | class GradeRepairChecks | - |
| Maps/SchoolFloor/Tests/Editor/NavigationExamChecks.cs | 212 | TheReturn.Editor | class NavigationExamChecks | - |
| Maps/SchoolFloor/Tests/Editor/QuietCorridorChecks.cs | 117 | TheReturn.Editor | class QuietCorridorChecks | - |
| Maps/SchoolFloor/Tests/Editor/TeacherTruthChecks.cs | 116 | TheReturn.Editor | class TeacherTruthChecks | - |
| Shared/Runtime/Interaction/PuzzleDoor.cs | 54 | TheReturn | class PuzzleDoor | 3: SchoolFloorBeforeUserBlockout.unity, SchoolFloorPrototype.unity, PF_GB_SlidingDoor.prefab |
| Shared/Runtime/UI/CanvasThemeBinding.cs | 36 | TheReturn | class CanvasThemeBinding | 3: PF_UI_Button.prefab, PF_UI_Panel.prefab, PF_SchoolFloorCanvas.prefab |
| Shared/Runtime/UI/SchoolUITheme.cs | 19 | TheReturn | class SchoolUITheme | 1: SchoolUITheme.asset |
