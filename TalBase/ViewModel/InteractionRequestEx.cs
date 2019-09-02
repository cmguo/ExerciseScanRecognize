using Prism.Interactivity.InteractionRequest;

namespace TalBase.ViewModel
{


    public static class InteractionRequestEx
    {
        public static int RaiseForResult(this InteractionRequest<Confirmation> request, Confirmation context)
        {
            request.Raise(context);
            return context.Result;
        }
    }
}
