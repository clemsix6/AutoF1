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
   - ML-Agents relies on specific TensorFlow functionalities that have limited compatibility with macOS
   - Apple Silicon (M1/M2) Macs faced additional compatibility layers through Rosetta 2
   - Environment variables and paths handling differs between macOS and other platforms

2. **Python Dependencies**:
   - Version incompatibilities with the required Python runtime (3.8 needed but conflicting with other system dependencies)
   - Protobuf compatibility issues across different systems (ML-Agents requires specific protobuf versions)
   - PyTorch installation problems on some platforms (CUDA dependencies on Windows, architecture-specific builds)
   - Virtual environment conflicts with system Python installations

3. **Communication Issues**: Persistent problems with the connection between the Unity Editor and the Python training environment where ML-Agents runs
   - Socket timeouts during training initialization
   - Inconsistent serialization/deserialization of model data
   - Process termination during training due to memory constraints

### Technical Hurdles in Detail

Our team encountered numerous specific errors that prevented successful model training:

- **Socket Communication Failures**: When trying to establish communication between the Unity Editor and Python training process, we frequently encountered `SocketException` errors with messages like "Connection refused" or "Connection reset by peer"
  
- **Protobuf Version Conflicts**: ML-Agents requires specific versions of protobuf (typically 3.6.0 to 3.8.0), but many team members had either newer versions (causing deprecation issues) or older versions (causing feature unavailability)

- **Mlagents CLI Issues**: Commands like `mlagents-learn` would frequently fail with cryptic error messages related to TensorFlow backend initialization or CUDA unavailability

- **CUDA/PyTorch Compatibility**: On Windows machines, installing the correct CUDA toolkit version to match the PyTorch requirements proved challenging, with driver conflicts preventing proper GPU utilization

- **Editor-Python Sync**: Even when connections would establish, synchronization issues between the timescales of the Unity physics simulation and the Python training algorithm would cause agents to behave erratically

- **Memory Limitations**: Training attempts on less powerful machines would frequently crash due to memory exhaustion, particularly when trying to increase the number of parallel environments for faster training

After multiple attempts at troubleshooting these issues across different machines and operating systems, we reached a point where continued focus on resolving the technical dependencies would have consumed the remaining project time without guaranteeing success.

These obstacles made it impossible to complete the full ML training cycle within the project timeframe, despite the agent script being fully implemented and theoretically functional.

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