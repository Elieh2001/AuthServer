import { useState, useEffect } from 'react';
import { Grid, Typography, Box, Card, CardContent, Fade, Grow, Skeleton } from '@mui/material';
import { People, Business, Apps, TrendingUp } from '@mui/icons-material';

const Dashboard = () => {
  const [stats, setStats] = useState({
    users: 0,
    tenants: 0,
    applications: 0,
    activeUsers: 0,
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Simulate API call
    setTimeout(() => {
      setStats({
        users: 156,
        tenants: 12,
        applications: 24,
        activeUsers: 89,
      });
      setLoading(false);
    }, 500);
  }, []);

  const StatCard = ({ title, value, icon, color, delay }) => (
    <Grow in={!loading} timeout={500} style={{ transformOrigin: '0 0 0' }} {...{ timeout: delay }}>
      <Card
        elevation={4}
        sx={{
          height: '100%',
          borderRadius: 3,
          background: `linear-gradient(135deg, ${color}15 0%, ${color}05 100%)`,
          border: '1px solid',
          borderColor: `${color}30`,
          transition: 'all 0.3s ease-in-out',
          '&:hover': {
            transform: 'translateY(-8px)',
            boxShadow: 8,
            borderColor: color,
          },
        }}
      >
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
            <Box>
              <Typography
                color="text.secondary"
                gutterBottom
                variant="overline"
                sx={{ fontWeight: 600, letterSpacing: 1 }}
              >
                {title}
              </Typography>
              <Typography variant="h3" sx={{ fontWeight: 'bold', color }}>
                {value.toLocaleString()}
              </Typography>
            </Box>
            <Box
              sx={{
                color,
                bgcolor: `${color}20`,
                borderRadius: '50%',
                p: 2,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              {icon}
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Grow>
  );

  if (loading) {
    return (
      <Box>
        <Skeleton variant="text" width={200} height={60} sx={{ mb: 4 }} />
        <Grid container spacing={3}>
          {[1, 2, 3, 4].map((i) => (
            <Grid item xs={12} sm={6} md={3} key={i}>
              <Skeleton variant="rectangular" height={120} sx={{ borderRadius: 3 }} />
            </Grid>
          ))}
        </Grid>
      </Box>
    );
  }

  return (
    <Fade in timeout={500}>
      <Box>
        <Typography
          variant="h4"
          gutterBottom
          sx={{ fontWeight: 'bold', color: 'primary.main', mb: 4 }}
        >
          Dashboard Overview
        </Typography>
        <Grid container spacing={3}>
          <Grid item xs={12} sm={6} md={3}>
            <StatCard
              title="Total Users"
              value={stats.users}
              icon={<People sx={{ fontSize: 40 }} />}
              color="#1976d2"
              delay={300}
            />
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <StatCard
              title="Tenants"
              value={stats.tenants}
              icon={<Business sx={{ fontSize: 40 }} />}
              color="#2e7d32"
              delay={400}
            />
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <StatCard
              title="Applications"
              value={stats.applications}
              icon={<Apps sx={{ fontSize: 40 }} />}
              color="#ed6c02"
              delay={500}
            />
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <StatCard
              title="Active Users"
              value={stats.activeUsers}
              icon={<TrendingUp sx={{ fontSize: 40 }} />}
              color="#0288d1"
              delay={600}
            />
          </Grid>
        </Grid>
      </Box>
    </Fade>
  );
};

export default Dashboard;
