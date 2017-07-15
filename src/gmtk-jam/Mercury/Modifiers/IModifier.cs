namespace gmtk_jam.Mercury.Modifiers {
    public interface IModifier {
        
        void Update(float elapsedSeconds, ParticleBuffer.ParticleIterator iterator);
    }
}