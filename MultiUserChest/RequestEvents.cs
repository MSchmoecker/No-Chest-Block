using System;

namespace MultiUserChest {
    /// <summary>
    /// Lets other mods follow chest transfers that Multi User Chest handles for them.
    /// Both events are raised on the requesting client only, on the main thread.
    /// Add and remove requests are reported; moves inside a chest, drops and consumes are not.
    /// </summary>
    public static class RequestEvents {
        /// <summary>
        /// Raised after an add or remove request was created and the requesting inventory was changed,
        /// before it is sent to the chest owner.
        /// Raised synchronously from the call that started the transfer (for example <c>Inventory.MoveItemToThis</c>),
        /// so the caller can read <see cref="IRequest.RequestID"/> to recognise the answer later.
        /// </summary>
        public static event Action<IRequest> RequestSent;

        /// <summary>
        /// Raised after the answer to a request has been applied to the requesting inventory.
        /// <see cref="IResponse.SourceID"/> equals the <see cref="IRequest.RequestID"/> passed to <see cref="RequestSent"/>.
        /// The response is a <see cref="RequestChestAddResponse"/> or a <see cref="RequestChestRemoveResponse"/>.
        /// The request can be null if it is no longer known.
        /// No event is raised for a request that never gets an answer.
        /// </summary>
        public static event Action<IRequest, IResponse> ResponseApplied;

        internal static void OnRequestSent(IRequest request) {
            if (RequestSent == null) {
                return;
            }

            foreach (Action<IRequest> handler in RequestSent.GetInvocationList()) {
                try {
                    handler(request);
                } catch (Exception e) {
                    Log.LogError($"RequestEvents: {nameof(RequestSent)} handler threw: {e}");
                }
            }
        }

        internal static void OnResponseApplied(IRequest request, IResponse response) {
            if (ResponseApplied == null) {
                return;
            }

            foreach (Action<IRequest, IResponse> handler in ResponseApplied.GetInvocationList()) {
                try {
                    handler(request, response);
                } catch (Exception e) {
                    Log.LogError($"RequestEvents: {nameof(ResponseApplied)} handler threw: {e}");
                }
            }
        }
    }
}
