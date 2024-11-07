from EventSystem import event_system

'''
Data will be used for three different purposes:
    - Live data: Images and current stats will be transfers directly to the cloud
    - Current behaviour: Data over from over a short period (last 10 minutes) will be analyzed and behaovior and well being will be determined
    - Long term behaviour / well being:
'''

class LiveDataModule:
    def __init__(self):
        event_system.subscribe(self.process_data, 'message_received')

    def process_data(self, data):
        pass