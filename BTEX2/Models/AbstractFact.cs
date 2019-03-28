

namespace BitCWallet.Models
{
    public abstract class AbstractProA { }
    public abstract class AbstractProB { }
    public class Base
    {
        private AbstractProA _productA;
        private AbstractProB _productb;
        public Base(AbstractFact fact)
        {
            _productA = fact.CreateProdA();
            _productb = fact.CreateProdB();
        }
    }
   
    public abstract class AbstractFact
    {
        public abstract AbstractProA CreateProdA();
        public abstract AbstractProB CreateProdB();
    }
   
    public class ConcFactA : AbstractFact
    {
        public override AbstractProA CreateProdA()
        {
            return new Prod1();
        }

        public override AbstractProB CreateProdB()
        {
            return new Prod2();
        }
    }
    public class Prod1 : AbstractProA
    {

    }
    public class Prod2 : AbstractProB
    {

    }
}