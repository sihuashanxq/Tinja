namespace Tinja.Test.Fakes
{
    public interface IParamterService
    {

    }

    public class ParamterServie : IParamterService
    {
        public ParamterServie(IConstructorCircularDepenencyService service)
        {

        }
    }

    public interface IConstructorCircularDepenencyService
    {

    }

    public class ConstructorCircularDepenencyService : IConstructorCircularDepenencyService
    {
        public ConstructorCircularDepenencyService(IParamterService service)
        {

        }
    }
}
