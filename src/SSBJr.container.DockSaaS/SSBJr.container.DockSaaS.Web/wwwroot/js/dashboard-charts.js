// Dashboard Charts JavaScript
// This file contains chart initialization and update functions for the DockSaaS dashboard

// Chart instances
let usageChart = null;
let serviceMetricsChart = null;

// Initialize charts when the page loads
window.initializeCharts = function() {
    try {
        // Initialize usage trends chart
        initializeUsageChart();
        
        // Initialize service metrics chart (for dialog)
        initializeServiceMetricsChart();
        
        console.log('Charts initialized successfully');
    } catch (error) {
        console.error('Error initializing charts:', error);
    }
};

// Initialize the usage trends chart
function initializeUsageChart() {
    const ctx = document.getElementById('usageChart');
    if (!ctx) return;

    const config = {
        type: 'line',
        data: {
            labels: [], // Will be populated with dates
            datasets: [
                {
                    label: 'Storage (GB)',
                    data: [],
                    borderColor: 'rgb(75, 192, 192)',
                    backgroundColor: 'rgba(75, 192, 192, 0.1)',
                    tension: 0.4,
                    fill: true
                },
                {
                    label: 'API Calls (K)',
                    data: [],
                    borderColor: 'rgb(255, 99, 132)',
                    backgroundColor: 'rgba(255, 99, 132, 0.1)',
                    tension: 0.4,
                    fill: true,
                    yAxisID: 'y1'
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false,
            },
            scales: {
                x: {
                    display: true,
                    title: {
                        display: true,
                        text: 'Date'
                    },
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                },
                y: {
                    type: 'linear',
                    display: true,
                    position: 'left',
                    title: {
                        display: true,
                        text: 'Storage (GB)'
                    },
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                },
                y1: {
                    type: 'linear',
                    display: true,
                    position: 'right',
                    title: {
                        display: true,
                        text: 'API Calls (K)'
                    },
                    grid: {
                        drawOnChartArea: false,
                    },
                }
            },
            plugins: {
                legend: {
                    position: 'top',
                },
                title: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            let label = context.dataset.label || '';
                            if (label) {
                                label += ': ';
                            }
                            if (context.parsed.y !== null) {
                                if (context.dataset.label.includes('Storage')) {
                                    label += formatBytes(context.parsed.y * 1024 * 1024 * 1024);
                                } else {
                                    label += (context.parsed.y * 1000).toLocaleString();
                                }
                            }
                            return label;
                        }
                    }
                }
            },
            animation: {
                duration: 750,
                easing: 'easeInOutQuart'
            }
        }
    };

    try {
        if (window.Chart) {
            usageChart = new Chart(ctx, config);
        } else {
            console.warn('Chart.js not loaded');
        }
    } catch (error) {
        console.error('Error creating usage chart:', error);
    }
}

// Initialize service metrics chart
function initializeServiceMetricsChart() {
    const ctx = document.getElementById('serviceMetricsChart');
    if (!ctx) return;

    const config = {
        type: 'line',
        data: {
            labels: [],
            datasets: []
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false,
            },
            scales: {
                x: {
                    display: true,
                    title: {
                        display: true,
                        text: 'Time (Hour)'
                    }
                },
                y: {
                    display: true,
                    title: {
                        display: true,
                        text: 'Value'
                    }
                }
            },
            plugins: {
                legend: {
                    position: 'top',
                },
                title: {
                    display: false
                }
            }
        }
    };

    try {
        if (window.Chart) {
            serviceMetricsChart = new Chart(ctx, config);
        }
    } catch (error) {
        console.error('Error creating service metrics chart:', error);
    }
}

// Update usage chart with new data
window.updateUsageChart = function(trendsData) {
    if (!usageChart || !trendsData) return;

    try {
        // Generate sample data for demonstration
        const labels = [];
        const storageData = [];
        const apiCallsData = [];

        // Generate last 7 days
        for (let i = 6; i >= 0; i--) {
            const date = new Date();
            date.setDate(date.getDate() - i);
            labels.push(date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }));
            
            // Generate realistic sample data
            storageData.push(Math.random() * 10 + i * 0.5); // Growing trend
            apiCallsData.push(Math.random() * 50 + i * 2); // Growing trend
        }

        usageChart.data.labels = labels;
        usageChart.data.datasets[0].data = storageData;
        usageChart.data.datasets[1].data = apiCallsData;
        
        usageChart.update('none'); // Update without animation for better performance
    } catch (error) {
        console.error('Error updating usage chart:', error);
    }
};

// Update service metrics chart
window.updateServiceMetricsChart = function(metricsData) {
    if (!serviceMetricsChart || !metricsData) return;

    try {
        const labels = [];
        const datasets = [];
        
        // Generate sample metrics data
        if (metricsData.Metrics) {
            const colors = [
                'rgb(255, 99, 132)',
                'rgb(54, 162, 235)',
                'rgb(255, 205, 86)',
                'rgb(75, 192, 192)',
                'rgb(153, 102, 255)',
                'rgb(255, 159, 64)'
            ];
            
            let colorIndex = 0;
            
            Object.keys(metricsData.Metrics).forEach(metricName => {
                const metricData = metricsData.Metrics[metricName];
                
                datasets.push({
                    label: formatMetricName(metricName),
                    data: metricData.map(d => d.AverageValue),
                    borderColor: colors[colorIndex % colors.length],
                    backgroundColor: colors[colorIndex % colors.length] + '20',
                    tension: 0.4,
                    fill: false
                });
                
                colorIndex++;
            });
            
            // Use hours as labels
            if (datasets.length > 0) {
                labels.push(...Array.from({length: 24}, (_, i) => `${i}:00`));
            }
        }

        serviceMetricsChart.data.labels = labels;
        serviceMetricsChart.data.datasets = datasets;
        serviceMetricsChart.update();
        
    } catch (error) {
        console.error('Error updating service metrics chart:', error);
    }
};

// Utility functions
function formatBytes(bytes) {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

function formatMetricName(metricName) {
    return metricName
        .replace(/_/g, ' ')
        .replace(/\b\w/g, l => l.toUpperCase());
}

// Real-time updates simulation
function startRealTimeUpdates() {
    setInterval(() => {
        if (usageChart) {
            // Simulate real-time data updates
            const datasets = usageChart.data.datasets;
            datasets.forEach(dataset => {
                const lastValue = dataset.data[dataset.data.length - 1];
                const newValue = lastValue + (Math.random() - 0.5) * 2;
                dataset.data.push(Math.max(0, newValue));
                
                // Keep only last 20 points
                if (dataset.data.length > 20) {
                    dataset.data.shift();
                }
            });
            
            // Update labels
            const now = new Date();
            usageChart.data.labels.push(now.toLocaleTimeString());
            if (usageChart.data.labels.length > 20) {
                usageChart.data.labels.shift();
            }
            
            usageChart.update('none');
        }
    }, 30000); // Update every 30 seconds
}

// Initialize real-time updates when charts are ready
document.addEventListener('DOMContentLoaded', function() {
    setTimeout(() => {
        startRealTimeUpdates();
    }, 2000);
});

// Resize charts when window is resized
window.addEventListener('resize', function() {
    if (usageChart) {
        usageChart.resize();
    }
    if (serviceMetricsChart) {
        serviceMetricsChart.resize();
    }
});

// Export functions for global access
window.dashboardCharts = {
    updateUsageChart: window.updateUsageChart,
    updateServiceMetricsChart: window.updateServiceMetricsChart,
    initializeCharts: window.initializeCharts
};