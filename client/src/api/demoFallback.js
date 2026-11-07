import demoData from '../data/demoData.json';

/**
 * In-memory fallback dataset for offline/portfolio demonstration
 * when the local ASP.NET Core backend is unreachable.
 */

export const getFallbackProducts = (params = {}) => {
  let list = [...demoData.products];

  // 1. Search filter
  if (params.search && typeof params.search === 'string' && params.search.trim()) {
    const q = params.search.trim().toLowerCase();
    list = list.filter(
      (p) =>
        (p.name && p.name.toLowerCase().includes(q)) ||
        (p.description && p.description.toLowerCase().includes(q)) ||
        (p.categoryName && p.categoryName.toLowerCase().includes(q))
    );
  }

  // 2. Category filter (including subcategories)
  if (params.categoryId) {
    const catId = Number(params.categoryId);
    const childIds = demoData.categories
      .filter((c) => c.parentCategoryId === catId)
      .map((c) => c.id);
    const allowed = [catId, ...childIds];
    list = list.filter((p) => allowed.includes(p.categoryId));
  }

  // 3. Price filter
  if (params.minPrice !== undefined && params.minPrice !== '' && !isNaN(params.minPrice)) {
    const min = Number(params.minPrice);
    list = list.filter((p) => (p.discountPrice || p.price) >= min);
  }
  if (params.maxPrice !== undefined && params.maxPrice !== '' && !isNaN(params.maxPrice)) {
    const max = Number(params.maxPrice);
    list = list.filter((p) => (p.discountPrice || p.price) <= max);
  }

  // 4. In-stock filter
  if (params.inStock === true || params.inStock === 'true') {
    list = list.filter((p) => p.stockQuantity > 0);
  }

  // 5. Sorting
  const sortBy = params.sortBy || 'newest';
  if (sortBy === 'price_asc') {
    list.sort((a, b) => (a.discountPrice || a.price) - (b.discountPrice || b.price));
  } else if (sortBy === 'price_desc') {
    list.sort((a, b) => (b.discountPrice || b.price) - (a.discountPrice || a.price));
  } else if (sortBy === 'name_asc') {
    list.sort((a, b) => (a.name || '').localeCompare(b.name || ''));
  } else if (sortBy === 'popularity') {
    list.sort((a, b) => (b.stockQuantity || 0) - (a.stockQuantity || 0));
  } else {
    // 'newest'
    list.sort((a, b) => (b.id || 0) - (a.id || 0));
  }

  // 6. Pagination
  const page = Math.max(1, parseInt(params.page || 1, 10));
  const pageSize = Math.max(1, parseInt(params.pageSize || 12, 10));
  const totalCount = list.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const startIndex = (page - 1) * pageSize;
  const items = list.slice(startIndex, startIndex + pageSize);

  return {
    success: true,
    data: {
      items,
      totalCount,
      totalPages,
      pageNumber: page,
      pageSize,
      hasPreviousPage: page > 1,
      hasNextPage: page < totalPages,
    },
  };
};

export const getFallbackProductById = (id) => {
  const numId = Number(id);
  const product = demoData.products.find((p) => p.id === numId || p.id === id);
  return {
    success: !!product,
    data: product || null,
  };
};

export const getFallbackProductBySlug = (slug) => {
  const product = demoData.products.find((p) => p.slug === slug);
  return {
    success: !!product,
    data: product || null,
  };
};

export const getFallbackCategories = () => {
  return {
    success: true,
    data: demoData.categories,
  };
};

export const getFallbackCategoryTree = () => {
  const roots = demoData.categories
    .filter((c) => c.parentCategoryId == null)
    .sort((a, b) => a.name.localeCompare(b.name));

  const tree = roots.map((root) => {
    const subs = demoData.categories
      .filter((c) => c.parentCategoryId === root.id)
      .sort((a, b) => a.name.localeCompare(b.name));

    return {
      ...root,
      subCategories: subs,
    };
  });

  return {
    success: true,
    data: tree,
  };
};

export const getFallbackCategoryById = (id) => {
  const numId = Number(id);
  const cat = demoData.categories.find((c) => c.id === numId || c.id === id);
  return {
    success: !!cat,
    data: cat || null,
  };
};

export const getFallbackReviews = (productId) => {
  return {
    success: true,
    data: {
      averageRating: 4.9,
      totalReviews: 3,
      reviews: [
        {
          id: 1,
          userName: 'Aysel Məmmədova',
          rating: 5,
          comment: 'Keyfiyyəti əladır, çox razı qaldım!',
          createdAt: new Date(Date.now() - 172800000).toISOString(),
        },
        {
          id: 2,
          userName: 'Rəşad Əliyev',
          rating: 5,
          comment: 'Gözlədiyimdən daha yaxşı çıxdı. Şəkillə tam eynidir.',
          createdAt: new Date(Date.now() - 432000000).toISOString(),
        },
        {
          id: 3,
          userName: 'Leyla Qasımova',
          rating: 4,
          comment: 'Sürətli çatdırılma və səliqəli paketləmə.',
          createdAt: new Date(Date.now() - 864000000).toISOString(),
        },
      ],
    },
  };
};

export const getFallbackAuthLogin = (credentials) => {
  const email = credentials?.email?.toLowerCase() || '';
  const isAdmin = email.includes('admin');
  const user = {
    id: isAdmin ? 'demo-superadmin-id' : 'demo-customer-id',
    email: credentials?.email || (isAdmin ? 'superadmin@shop.com' : 'customer@shop.com'),
    firstName: isAdmin ? 'Super' : 'Customer',
    lastName: isAdmin ? 'Admin' : 'User',
    roles: isAdmin ? ['SuperAdmin', 'Admin'] : ['Customer'],
    permissions: isAdmin
      ? [
          'Product.View',
          'Product.Manage',
          'Category.Manage',
          'Order.Manage',
          'User.Manage',
          'Role.Manage',
          'Statistics.View',
          'Trash.Manage',
        ]
      : ['Product.View'],
  };

  // Create standard JWT payload for jwt-decode compatibility
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  const payload = btoa(
    JSON.stringify({
      sub: user.id,
      email: user.email,
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': user.roles,
      permission: user.permissions,
      exp: Math.floor(Date.now() / 1000) + 86400 * 7,
    })
  );
  const token = `${header}.${payload}.demoSignature`;

  return {
    succeeded: true,
    message: 'Login successful (Demo Mode)',
    data: {
      accessToken: token,
      refreshToken: 'demo-refresh-token',
      user,
    },
  };
};

export const getFallbackCurrentUser = () => {
  return {
    succeeded: true,
    data: {
      id: 'demo-superadmin-id',
      email: 'superadmin@shop.com',
      firstName: 'Super',
      lastName: 'Admin',
      roles: ['SuperAdmin', 'Admin'],
      permissions: [
        'Product.View',
        'Product.Manage',
        'Category.Manage',
        'Order.Manage',
        'User.Manage',
        'Role.Manage',
        'Statistics.View',
        'Trash.Manage',
      ],
    },
  };
};

export const getFallbackStatisticsSummary = () => {
  return {
    success: true,
    data: {
      totalRevenue: 124850.75,
      monthlyRevenue: 18450.2,
      totalOrders: 342,
      pendingOrders: 14,
      totalCustomers: 128,
      totalProducts: demoData.products.length,
      lowStockCount: 3,
    },
  };
};

export const getFallbackStatisticsSales = () => {
  return {
    success: true,
    data: [
      { period: 'May', revenue: 9500, ordersCount: 28 },
      { period: 'Jun', revenue: 11200, ordersCount: 34 },
      { period: 'Jul', revenue: 14800, ordersCount: 42 },
      { period: 'Aug', revenue: 13600, ordersCount: 38 },
      { period: 'Sep', revenue: 17200, ordersCount: 51 },
      { period: 'Oct', revenue: 18450, ordersCount: 56 },
    ],
  };
};

export const getFallbackTopProducts = (top = 5) => {
  return {
    success: true,
    data: demoData.products.slice(0, top).map((p, idx) => ({
      productId: p.id,
      productName: p.name,
      imageUrl: p.imageUrl,
      price: p.price,
      unitsSold: 95 - idx * 12,
      totalRevenue: (p.discountPrice || p.price) * (95 - idx * 12),
    })),
  };
};

export const getFallbackCategoryStats = () => {
  return {
    success: true,
    data: demoData.categories.map((c) => ({
      name: c.name,
      productCount: c.productCount || 5,
      revenue: (c.productCount || 5) * 1420,
    })),
  };
};

export const getFallbackOrdersByStatus = () => {
  return {
    success: true,
    data: [
      { status: 'Completed', count: 260 },
      { status: 'Processing', count: 48 },
      { status: 'Pending', count: 22 },
      { status: 'Cancelled', count: 12 },
    ],
  };
};
