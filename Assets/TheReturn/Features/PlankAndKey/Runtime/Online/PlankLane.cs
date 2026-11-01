/*
 * Mục đích: Hình học của hành lang cầu ván dùng chung cho host và máy khách (T-027): vị trí ván theo tiến độ, điểm đứng đầu khe, vị trí thẻ theo
 * trạng thái, vùng ván. Đọc các điểm mốc từ PlankAndKeyPrototype có sẵn trong scene (laneOrigin, deck, deckZone, deckRestA/B, anchorA, card,
 * cardHomeA). Online luôn coi bờ A là bờ đầu khe (nơi đội bắt đầu). Không giữ trạng thái.
 * Hàm: IsReady; DeckRest; DeckRestRotation; DeckPosition; StandPoint; StartYaw; ToLane; InDeckZone; CardPosition.
 */
using UnityEngine;

namespace TheReturn.PlankNet.Online
{
    public sealed class PlankLane
    {
        private readonly PlankAndKeyPrototype proto;

        /// <summary>Nhận PlankAndKeyPrototype chứa các điểm mốc.</summary>
        public PlankLane(PlankAndKeyPrototype prototype)
        {
            proto = prototype;
        }

        /// <summary>True nếu mọi điểm mốc cần thiết đã được nối trong scene.</summary>
        public bool IsReady
        {
            get
            {
                return proto != null && proto.laneOrigin != null && proto.deck != null && proto.deckZone != null && proto.deckRestA != null
                    && proto.deckRestB != null && proto.anchorA != null && proto.card != null && proto.cardHomeA != null;
            }
        }

        /// <summary>Nhận cờ bờ đầu khe (true) hay bờ bên kia (false); trả điểm đậu của ván ở bờ đó.</summary>
        public Vector3 DeckRest(bool atNear)
        {
            return (atNear ? proto.deckRestA : proto.deckRestB).position;
        }

        /// <summary>Trả hướng xoay của ván (giữ nguyên hướng điểm đậu A).</summary>
        public Quaternion DeckRestRotation()
        {
            return proto.deckRestA.rotation;
        }

        /// <summary>Nhận trạng thái và số giây còn lại của lượt chạy (đã ngoại suy); trả vị trí ván lúc này.</summary>
        public Vector3 DeckPosition(PlankPublicState state, float secondsLeft)
        {
            Vector3 from = DeckRest(state.DeckAtNear);
            if (state.Phase != PlankPhase.Crossing) return from;
            Vector3 to = DeckRest(!state.DeckAtNear);
            float total = state.CrossingSecondsTotal;
            float t = total <= 0f ? 1f : Mathf.Clamp01(1f - secondsLeft / total);
            return Vector3.Lerp(from, to, t);
        }

        /// <summary>Nhận ô và số người; trả điểm đứng ở bờ đầu khe (trải đều theo chiều ngang).</summary>
        public Vector3 StandPoint(int slot, int count)
        {
            Transform a = proto.anchorA;
            float spread = (slot - (count - 1) * 0.5f) * 1.2f;
            return a.position + a.right * spread;
        }

        /// <summary>Trả góc xoay quanh trục Y để cả đội nhìn về bờ bên kia.</summary>
        public float StartYaw()
        {
            Vector3 toward = proto.deckRestB.position - proto.deckRestA.position;
            toward.y = 0f;
            return Mathf.Atan2(toward.x, toward.z) * Mathf.Rad2Deg;
        }

        /// <summary>Nhận điểm thế giới; trả toạ độ trong hệ làn (x ngang, z dọc hành lang).</summary>
        public Vector3 ToLane(Vector3 world)
        {
            return proto.laneOrigin.InverseTransformPoint(world);
        }

        /// <summary>Nhận điểm thế giới; trả true nếu điểm nằm trong vùng ván.</summary>
        public bool InDeckZone(Vector3 world)
        {
            BoxCollider box = proto.deckZone;
            if (box == null) return false;
            return new Bounds(box.center, box.size).Contains(box.transform.InverseTransformPoint(world));
        }

        /// <summary>Nhận trạng thái, vị trí ván lúc này và thân người đang cầm thẻ (null nếu không ai); trả vị trí thẻ từ.</summary>
        public Vector3 CardPosition(PlankPublicState state, Vector3 deckPosition, Transform holder)
        {
            switch (state.CardMode)
            {
                case PlankCardMode.Held:
                    if (holder != null) return holder.position + Vector3.up * 1.15f + holder.right * 0.35f + holder.forward * 0.45f;
                    return proto.cardHomeA.position;
                case PlankCardMode.OnFloor:
                    return proto.laneOrigin.TransformPoint(new Vector3(state.CardLocalX, 0.12f, state.CardLocalZ));
                case PlankCardMode.OnDeck:
                    return deckPosition + proto.laneOrigin.right * state.CardLocalX + proto.laneOrigin.forward * state.CardLocalZ + Vector3.up * 0.3f;
                default:
                    return proto.cardHomeA.position;
            }
        }
    }
}
