## Notes on Brain Design Ideas


struct Nuerotransmitter
{
	/// <summary>
	/// Neurons in the core of the brain release dopamine (DO-pa-meen), a neurotransmitter that affects processes that control movement, emotional response, and the ability to experience pleasure and pain. 
    /// Read more: http://www.humanillnesses.com/knowledge/Dopamine.html	
	/// This can be thought of as the multiplier for unexpected stimuli and how much effect it should have on learning e.g.
	/// Bump sensor trigger might be multiplied in the negative if the brain is overly sensitive.
	/// </summary>
	public double Dopamine;
		
	/// <summary>
	/// Gamma-aminobutyric acid, or GABA, is the main neurotransmitter that works to inhibit the brain's neurons from acting. Research suggests that certain types of epilepsy, which is characterized by recurring seizures that affect a person's awareness and movements, may be the result of having too little GABA in the brain. The neuronal messaging system goes into overdrive, with tens of thousands of neurons sending messages intensely and simultaneously, which produces a seizure. Researchers believe that enzymes may be responsible for breaking down too much GABA, and they have developed medications that appear to help combat this process.
	/// Read more: http://www.humanillnesses.com/Behavioral-Health-A-Br/Brain-Chemistry-Neurochemistry.html#ixzz3n8Vgg7AS
	/// </summary>
	public double Gaba;
	
	/// <summary>
	/// Norepinephrine (nor-e-pi-NE-frin) is a neurotransmitter that is involved in various arousal systems in the brain (systems that bring about alertness and attention) and in the sympathetic nervous system * . In the sympathetic nervous system, it is norepinephrine that causes the blood vessels to narrow, raising blood pressure, and speeds the breathing and heart rates.
	/// Read more: http://www.humanillnesses.com/Behavioral-Health-A-Br/Brain-Chemistry-Neurochemistry.html#ixzz3n8W2MFhi
	/// </summary>
	public double Norepinephrine;
	
	/// <summary>
	/// Many studies have linked low levels of the neurotransmitter serotonin to depression, impulsive and aggressive forms of behavior, violence, and even suicide
	/// Read more: http://www.humanillnesses.com/Behavioral-Health-A-Br/Brain-Chemistry-Neurochemistry.html#ixzz3n8WrSpAu
	/// </summary>
	public double Seratonin;
}


* Noise generator to give brain unpredictability (searching / imaginative behaviour)
* Inhibitor monitors brain activity and activity to induce GABA (dulling the system when it becomes over-active)
* Clock cell provides a time / age based component where values are provided based on a stride offset and constant age deltas e.g 1 second age +/-100ms
* Hippocampus is used to preserve the state of the neurons (has place cells for spatial reasoning)
* Adaptation neuron attempts to adjust strides instead of learning (perhaps using a genetic algorithm approach to learning vs re-adapting existing nuerons?)

Problems
* We need a way of determining appropriate chemical responses to environmental inputs. e.g. 
  * If the robot senses a long + soft touch, this could induce an increase in seratonin and dopamine, leading to a calm, docile and happy robot. 
  * Conversely a quick + hard touch could induce a large increase in norepinephrine and a reduction in seratonin, leading to a fearful or angry robot (fight / flight response)
