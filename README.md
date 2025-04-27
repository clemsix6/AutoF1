# Autonomous F1 Racing with Unity ML-Agents

## Project Overview
This Unity project aims to create an autonomous Formula 1 car using machine learning techniques through Unity's ML-Agents framework. The car is designed to learn how to navigate a race track optimally by receiving rewards for making progress along the track and penalties for mistakes like going off-track or colliding with walls.

## Technical Implementation

### ML-Agents Architecture
The project is built around the `CarAgent.cs` script, which implements the core ML-Agents functionality:

- **Observation System**: The agent collects information about its environment including:
  - Current velocity (normalized)
  - Direction and distance to the next checkpoint
  - Relative orientation to upcoming checkpoints

- **Action System**: The agent can control:
  - Steering (continuous action between -1 and 1)
  - Throttle/braking (continuous action between -1 and 1)

- **Reward Structure**:
  - Positive rewards for reaching checkpoints and maintaining course
  - Negative rewards for going off-track, hitting walls, or taking too long
  - Small time penalty to encourage efficient path-finding

- **Reset Conditions**:
  - Episodes end when the car completes a lap, hits a wall, or goes severely off-track

### Physics Configuration
The car uses Unity's physics system with customized settings:
- Lower center of mass for better stability
- Configurable acceleration and turn forces
- Maximum velocity capping for realistic behavior

## Implementation Challenges

Despite having a well-structured implementation, we encountered several significant challenges that prevented us from fully training the ML model:

1. **Platform Compatibility**: Unable to run the ML-Agents training on macOS, which was the primary development environment for most team members

2. **Python Dependencies**:
   - Version incompatibilities with the required Python runtime
   - Protobuf compatibility issues across different systems
   - PyTorch installation problems on some platforms

3. **Communication Issues**: Persistent problems with the connection between the Unity Editor and the Python training environment where ML-Agents runs

These obstacles made it impossible to complete the full ML training cycle within the project timeframe.

## Fallback Solution

To ensure a deliverable product, we implemented a simpler waypoint-following system as a fallback:
- Predetermined path defined by waypoints placed around the track
- The car follows these points using standard steering algorithms
- No machine learning involved, but provides a functional demonstration

The waypoint system is implemented via the `WaypointGenerator.cs` and related scripts.

## Setup Requirements

### For Running the Waypoint Demo:
- Unity 2021.3 or newer
- Standard Unity built-in render pipeline (or URP)
- No additional dependencies required

### For ML-Agents Training (Currently Non-Functional):
- Unity ML-Agents package
- Python 3.8+
- PyTorch
- ML-Agents Python package (`mlagents`)
- Protobuf compatibility with ML-Agents version

## Conclusion

While we weren't able to fully implement the ML training component due to technical constraints, the project provides a solid foundation for autonomous vehicle simulation in Unity. The CarAgent script contains all the necessary components to work with ML-Agents and could be successfully deployed with the right environment configuration.

The fallback waypoint system demonstrates the intended behavior and provides a functional baseline that could be compared against future ML implementations.