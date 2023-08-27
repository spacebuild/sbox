namespace Sdt.Environment;

public interface GravitationalAwarePlayer
{
    
    public IGravitationalEnvironment CurrentEnvironment { get; set; }
    
    public Vector3 Gravity { get; set; }
    
    public Vector3 GravityDirection { get; set; }
    
}