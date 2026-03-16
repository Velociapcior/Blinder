---
name: postgresql-optimization
description: 'PostgreSQL-specific development assistant focusing on unique PostgreSQL features, advanced data types, and PostgreSQL-exclusive capabilities. Covers JSONB operations, array types, custom types, range/geometric types, full-text search, window functions, and PostgreSQL extensions ecosystem.'
---

# PostgreSQL Development Assistant

Expert PostgreSQL guidance for ${selection} (or entire project if no selection). Focus on PostgreSQL-specific features, optimization patterns, and advanced capabilities.

## PostgreSQL-Specific Features

### JSONB Operations
```sql
-- Advanced JSONB queries
CREATE TABLE events (
    id SERIAL PRIMARY KEY,
    data JSONB NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- GIN index for JSONB performance
CREATE INDEX idx_events_data_gin ON events USING gin(data);

-- JSONB containment and path queries
SELECT * FROM events 
WHERE data @> '{"type": "login"}'
  AND data #>> '{user,role}' = 'admin';
```

### Array Operations
```sql
-- PostgreSQL arrays
CREATE TABLE posts (
    id SERIAL PRIMARY KEY,
    tags TEXT[],
    categories INTEGER[]
);

-- Array queries and operations
SELECT * FROM posts WHERE 'postgresql' = ANY(tags);
SELECT * FROM posts WHERE tags && ARRAY['database', 'sql'];
```

### Window Functions & Analytics
```sql
-- Advanced window functions
SELECT 
    product_id,
    sale_date,
    amount,
    SUM(amount) OVER (PARTITION BY product_id ORDER BY sale_date) as running_total,
    AVG(amount) OVER (PARTITION BY product_id ORDER BY sale_date ROWS BETWEEN 2 PRECEDING AND CURRENT ROW) as moving_avg,
    DENSE_RANK() OVER (PARTITION BY EXTRACT(month FROM sale_date) ORDER BY amount DESC) as monthly_rank,
    LAG(amount, 1) OVER (PARTITION BY product_id ORDER BY sale_date) as prev_amount
FROM sales;
```

### Full-Text Search
```sql
-- PostgreSQL full-text search
CREATE TABLE documents (
    id SERIAL PRIMARY KEY,
    title TEXT,
    content TEXT,
    search_vector tsvector
);

-- GIN index for search performance
CREATE INDEX idx_documents_search ON documents USING gin(search_vector);

-- Search queries with ranking
SELECT *, ts_rank(search_vector, plainto_tsquery('postgresql')) as rank
FROM documents 
WHERE search_vector @@ plainto_tsquery('english', 'postgresql database')
ORDER BY rank DESC;
```

## PostgreSQL Performance Tuning

### Query Optimization
```sql
-- EXPLAIN ANALYZE for performance analysis
EXPLAIN (ANALYZE, BUFFERS, FORMAT TEXT) 
SELECT u.name, COUNT(o.id) as order_count
FROM users u
LEFT JOIN orders o ON u.id = o.user_id
WHERE u.created_at > '2024-01-01'::date
GROUP BY u.id, u.name;

-- Identify slow queries from pg_stat_statements
SELECT query, calls, total_time, mean_time, rows,
       100.0 * shared_blks_hit / nullif(shared_blks_hit + shared_blks_read, 0) AS hit_percent
FROM pg_stat_statements 
ORDER BY total_time DESC 
LIMIT 10;
```

### Index Strategies
```sql
-- Composite indexes for multi-column queries
CREATE INDEX idx_orders_user_date ON orders(user_id, order_date);

-- Partial indexes for filtered queries
CREATE INDEX idx_active_users ON users(created_at) WHERE status = 'active';

-- Expression indexes for computed values
CREATE INDEX idx_users_lower_email ON users(lower(email));

-- Covering indexes to avoid table lookups
CREATE INDEX idx_orders_covering ON orders(user_id, status) INCLUDE (total, created_at);
```

## Advanced Data Types

### Custom Types & Domains
```sql
-- Create custom types
CREATE TYPE order_status AS ENUM ('pending', 'processing', 'shipped', 'delivered', 'cancelled');

-- Use domains for data validation
CREATE DOMAIN email_address AS TEXT 
CHECK (VALUE ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$');
```

### Range Types
```sql
-- PostgreSQL range types
CREATE TABLE reservations (
    id SERIAL PRIMARY KEY,
    room_id INTEGER,
    reservation_period tstzrange,
    price_range numrange
);

-- Range queries
SELECT * FROM reservations 
WHERE reservation_period && tstzrange('2024-07-20', '2024-07-25');

-- Exclude overlapping ranges
ALTER TABLE reservations 
ADD CONSTRAINT no_overlap 
EXCLUDE USING gist (room_id WITH =, reservation_period WITH &&);
```

### Geometric Types (PostGIS / NetTopologySuite)
```sql
-- GiST index for geometric / spatial data
CREATE INDEX idx_locations_coords ON locations USING gist(coordinates);

-- Spatial proximity query
SELECT * FROM users
WHERE ST_DWithin(location::geography, ST_SetSRID(ST_MakePoint(-73.935, 40.730), 4326)::geography, 10000);
```

## Common Query Patterns

### Cursor-Based Pagination
```sql
-- ❌ BAD: OFFSET for large datasets
SELECT * FROM products ORDER BY id OFFSET 10000 LIMIT 20;

-- ✅ GOOD: Cursor-based pagination
SELECT * FROM products 
WHERE id > $last_id 
ORDER BY id 
LIMIT 20;
```

### JSON Queries
```sql
-- ❌ BAD: Inefficient JSON querying
SELECT * FROM users WHERE data::text LIKE '%admin%';

-- ✅ GOOD: JSONB operators and GIN index
CREATE INDEX idx_users_data_gin ON users USING gin(data);
SELECT * FROM users WHERE data @> '{"role": "admin"}';
```

### CTEs for Hierarchical Data
```sql
-- Recursive CTE
WITH RECURSIVE category_tree AS (
    SELECT id, name, parent_id, 1 as level
    FROM categories 
    WHERE parent_id IS NULL
    
    UNION ALL
    
    SELECT c.id, c.name, c.parent_id, ct.level + 1
    FROM categories c
    JOIN category_tree ct ON c.parent_id = ct.id
)
SELECT * FROM category_tree ORDER BY level, name;
```

## Extensions & Tools

```sql
-- Enable commonly used extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";    -- UUID generation
CREATE EXTENSION IF NOT EXISTS "pgcrypto";     -- Cryptographic functions
CREATE EXTENSION IF NOT EXISTS "pg_trgm";      -- Trigram matching for fuzzy search
CREATE EXTENSION IF NOT EXISTS "postgis";      -- Spatial types and functions
```

## Optimization Checklist

### Query Analysis
- [ ] Run `EXPLAIN ANALYZE` for expensive queries
- [ ] Check for sequential scans on large tables
- [ ] Verify appropriate join algorithms
- [ ] Review WHERE clause selectivity
- [ ] Analyze sort and aggregation operations

### Index Strategy
- [ ] Create indexes for frequently queried columns
- [ ] Use composite indexes for multi-column searches
- [ ] Consider partial indexes for filtered queries
- [ ] Remove unused or duplicate indexes
- [ ] Monitor index bloat and fragmentation

### Security Review
- [ ] Use parameterized queries exclusively
- [ ] Implement proper access controls
- [ ] Enable row-level security where needed
- [ ] Audit sensitive data access

### Performance Monitoring
- [ ] Set up query performance monitoring via `pg_stat_statements`
- [ ] Configure `pgbouncer` for connection pooling in high-concurrency scenarios
- [ ] Monitor connection pool usage
- [ ] Schedule regular VACUUM and ANALYZE

Focus on providing specific, actionable PostgreSQL optimizations that improve query performance, security, and maintainability while leveraging PostgreSQL's advanced features.
