import pandas as pd
import matplotlib.pyplot as plt

def plot_all():
    # Read csv files
    pyt = pd.read_csv('python_times.csv', index_col=0)
    cpu = pd.read_csv('unity_times_cpu.csv', index_col=0)
    step_skipping = pd.read_csv('unity_times_step_skipping.csv', index_col=0)
    gpu = pd.read_csv('unity_times_gpu.csv', index_col=0)

    # Calculate means and plot
    plt.plot(pyt.mean(axis=1), label='Python')
    plt.plot(cpu.mean(axis=1), label='(Unity) on CPU')
    plt.plot(step_skipping.mean(axis=1), label='(Unity) on CPU with step skipping')
    plt.plot(gpu.mean(axis=1), label='(Unity) on GPU')

    plt.legend()
    plt.xlabel('Number of bodies')
    plt.ylabel('Time for 1 iteration [s]')
    plt.title('Comparison of force computation time')
    plt.show()
    
def plot_unity():
    # Read csv files
    cpu = pd.read_csv('unity_times_cpu.csv', index_col=0)
    step_skipping = pd.read_csv('unity_times_step_skipping.csv', index_col=0)
    gpu = pd.read_csv('unity_times_gpu.csv', index_col=0)

    # Calculate means and plot
    plt.plot(cpu.mean(axis=1), label='(Unity) on CPU')
    plt.plot(step_skipping.mean(axis=1), label='(Unity) on CPU with step skipping')
    plt.plot(gpu.mean(axis=1), label='(Unity) on GPU')

    plt.legend()
    plt.xlabel('Number of bodies')
    plt.ylabel('Time for 1 iteration [s]')
    plt.title('Comparison of force computation time (Unity implementations only)')
    plt.show()

if __name__ == '__main__':
    plot_all()
    plot_unity()
